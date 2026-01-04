using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VehicleTracking.DataGenerator.Models;

namespace VehicleTracking.DataGenerator.Services
{
	public class VehicleApiClient
	{
		private readonly HttpClient _httpClient;
		private readonly string _baseUrl;

		public VehicleApiClient(string baseUrl)
		{
			_baseUrl = baseUrl.TrimEnd('/');
			_httpClient = new HttpClient
			{
				Timeout = TimeSpan.FromSeconds(30)
			};
			_httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
		}

		public async Task<List<VehicleWithLastPosition>> GetVehiclesWithLastPositionsAsync()
		{
			var url = $"{_baseUrl}/api/vehicles/with-last-positions";
			try
			{
				Console.WriteLine($"  → Calling: {url}");
				var response = await _httpClient.GetAsync(url);

				if (!response.IsSuccessStatusCode)
				{
					Console.WriteLine($"  ⚠ API returned status {response.StatusCode}");
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
				Console.WriteLine($"  ⚠ Error fetching vehicles from {url}: {message}");
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

				var response = await _httpClient.PostAsync(url, content);

				if (!response.IsSuccessStatusCode)
				{
					var errorBody = await response.Content.ReadAsStringAsync();
					Console.WriteLine($"  ⚠ Error submitting positions for vehicle {vehicleId}. Status: {response.StatusCode}, Response: {errorBody}");
					return false;
				}

				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"  ⚠ Exception submitting positions for vehicle {vehicleId} to {url}: {ex.Message}");
				return false;
			}
		}
	}
}