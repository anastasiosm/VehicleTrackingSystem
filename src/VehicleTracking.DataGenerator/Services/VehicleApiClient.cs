using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using Serilog;
using VehicleTracking.Application.Dtos;
using VehicleTracking.DataGenerator.Dtos;

namespace VehicleTracking.DataGenerator.Services
{
	/// <summary>
	/// HTTP client for Vehicle Tracking API with resilience policies.
	/// Implements retry logic using Polly for transient failures.
	/// </summary>
	public class VehicleApiClient : IVehicleApiClient, IDisposable
	{
		// Shared HttpClient instance for better performance and socket management
		private static readonly HttpClient _sharedHttpClient = new HttpClient
		{
			Timeout = TimeSpan.FromSeconds(30)
		};

		private readonly string _baseUrl;
		private readonly ILogger _logger;
		private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
		private bool _disposed = false;

		private const int HTTP_TIMEOUT_SECONDS = 30;
		private const int MAX_RETRY_ATTEMPTS = 3;
		private const int RETRY_DELAY_SECONDS = 2;
		private const int HTTP_STATUS_TOO_MANY_REQUESTS = 429; // Not available in .NET 4.6.1

		static VehicleApiClient()
		{
			// Configure shared HttpClient once
			_sharedHttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
		}

		public VehicleApiClient(string baseUrl, ILogger logger)
		{
			_baseUrl = baseUrl?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(baseUrl));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			// Configure Polly retry policy for transient failures
			_retryPolicy = Policy
				.HandleResult<HttpResponseMessage>(r => 
					!r.IsSuccessStatusCode && IsTransientFailure(r.StatusCode))
				.Or<HttpRequestException>()
				.Or<TaskCanceledException>()
				.WaitAndRetryAsync(
					retryCount: MAX_RETRY_ATTEMPTS,
					sleepDurationProvider: retryAttempt => 
						TimeSpan.FromSeconds(Math.Pow(RETRY_DELAY_SECONDS, retryAttempt)), // Exponential backoff
					onRetry: (outcome, timespan, retryCount, context) =>
					{
						_logger.Warning(
							"Request failed. Waiting {Delay}s before retry #{RetryCount}. Reason: {Reason}",
							timespan.TotalSeconds, retryCount, 
							outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
					});
		}

		/// <summary>
		/// Determines if an HTTP status code represents a transient failure that should be retried.
		/// </summary>
		private static bool IsTransientFailure(HttpStatusCode statusCode)
		{
			// Cast to int for comparison since TooManyRequests enum doesn't exist in .NET 4.6.1
			int statusCodeValue = (int)statusCode;

			return statusCode == HttpStatusCode.RequestTimeout ||
				   statusCodeValue == HTTP_STATUS_TOO_MANY_REQUESTS || // 429
				   statusCode == HttpStatusCode.InternalServerError ||
				   statusCode == HttpStatusCode.BadGateway ||
				   statusCode == HttpStatusCode.ServiceUnavailable ||
				   statusCode == HttpStatusCode.GatewayTimeout;
		}

		public async Task<List<VehicleWithLastPosition>> GetVehiclesWithLastPositionsAsync()
		{
			var url = $"{_baseUrl}/api/vehicles/with-last-positions";
			try
			{
				_logger.Debug("Calling API: {Url}", url);

				// Execute request with retry policy
				var response = await _retryPolicy.ExecuteAsync(async () => 
					await _sharedHttpClient.GetAsync(url));

				if (!response.IsSuccessStatusCode)
				{
					_logger.Warning("API returned status {StatusCode} for URL {Url}", response.StatusCode, url);
					return new List<VehicleWithLastPosition>();
				}

				var json = await response.Content.ReadAsStringAsync();
				var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<VehicleWithLastPositionResponse>>>(json);

				if (apiResponse?.Success == true && apiResponse.Data != null)
				{
					return apiResponse.Data.Select(d => new VehicleWithLastPosition
					{
						Id = d.Vehicle.Id,
						Name = d.Vehicle.Name,
						IsActive = d.Vehicle.IsActive,
						LastPosition = d.LastPosition
					}).ToList();
				}

				return new List<VehicleWithLastPosition>();
			}
			catch (Exception ex)
			{
				var message = ex.InnerException != null ? $"{ex.Message} ({ex.InnerException.Message})" : ex.Message;
				_logger.Error(ex, "Error fetching vehicles from {Url}: {Message}", url, message);
				return new List<VehicleWithLastPosition>();
			}
		}

		public async Task<bool> SubmitPositionsBatchAsync(int vehicleId, List<GpsPositionData> positions)
		{
			var url = $"{_baseUrl}/api/gps/positions/batch";
			try
			{
				var request = new SubmitGpsPositionBatchRequest
				{
					VehicleId = vehicleId,
					Positions = positions
				};

				var json = JsonConvert.SerializeObject(request);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				// Execute request with retry policy
				var response = await _retryPolicy.ExecuteAsync(async () => 
					await _sharedHttpClient.PostAsync(url, content));

				if (!response.IsSuccessStatusCode)
				{
					var errorBody = await response.Content.ReadAsStringAsync();
					_logger.Error("Error submitting positions for vehicle {VehicleId}. Status: {StatusCode}, Response: {ErrorBody}", 
						vehicleId, response.StatusCode, errorBody);
					return false;
				}

				return true;
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "Exception submitting positions for vehicle {VehicleId} to {Url}", vehicleId, url);
				return false;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					// Cleanup managed resources if needed
					// Note: We don't dispose _sharedHttpClient as it's static and shared
				}
				_disposed = true;
			}
		}
	}
}