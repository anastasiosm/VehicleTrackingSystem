using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using VehicleTracking.Application.Interfaces;
using VehicleTracking.Application.Dtos;
using VehicleTracking.DataGenerator.Dtos;
using VehicleTracking.DataGenerator.Services;

namespace VehicleTracking.DataGenerator
{
	public class GpsDataGenerator
	{
		private readonly GeneratorConfig _config;
		private readonly IVehicleApiClient _apiClient;
		private readonly IPositionSimulator _simulator;
		private readonly ILogger _logger;

		public GpsDataGenerator(
			GeneratorConfig config, 
			IVehicleApiClient apiClient,
			IPositionSimulator simulator,
			ILogger logger)
		{
			_config = config;
			_apiClient = apiClient;
			_simulator = simulator;
			_logger = logger;
		}

		public async Task<GenerationResult> GenerateAndSubmitPositionsAsync()
		{
			_logger.Information("Fetching vehicles for GPS data generation...");
			var vehicles = await _apiClient.GetVehiclesWithLastPositionsAsync();

			if (vehicles == null || !vehicles.Any())
			{
				_logger.Warning("No vehicles found. Process terminated.");
				return new GenerationResult();
			}

			_logger.Information("System check: Found {Count} vehicles. Starting parallel processing...", vehicles.Count);

			// Parallel processing without locks
			var tasks = vehicles.Select(v => ProcessSingleVehicleAsync(v));
			var results = await Task.WhenAll(tasks);

			return AggregateResults(results);
		}

		private async Task<VehicleProcessResult> ProcessSingleVehicleAsync(VehicleWithLastPosition vehicle)
		{
			try
			{
				var startPoint = GetStartPoint(vehicle);
				
				var positions = _simulator.GeneratePath(startPoint, _config.PositionsPerVehicle, _config.RadiusMeters);
				
				var success = await _apiClient.SubmitPositionsBatchAsync(vehicle.Id, positions);

				if (!success)
				{
					_logger.Warning("API submission failed for vehicle {VehicleName} (ID: {VehicleId})", vehicle.Name, vehicle.Id);
				}

				return new VehicleProcessResult { IsSuccess = success, Count = success ? positions.Count : 0 };
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "Critical error processing vehicle {VehicleId}", vehicle.Id);
				return new VehicleProcessResult { IsSuccess = false, Count = 0 };
			}
		}

		private GpsPositionData GetStartPoint(VehicleWithLastPosition vehicle)
		{
			if (vehicle.LastPosition == null)
			{
				return _simulator.GetDefaultStartingPoint();
			}

			return new GpsPositionData
			{
				Latitude = vehicle.LastPosition.Latitude,
				Longitude = vehicle.LastPosition.Longitude,
				RecordedAt = vehicle.LastPosition.RecordedAt
			};
		}

		private GenerationResult AggregateResults(IEnumerable<VehicleProcessResult> results)
		{
			var finalResult = new GenerationResult();
			foreach (var res in results)
			{
				if (res.IsSuccess)
				{
					finalResult.VehiclesProcessed++;
					finalResult.TotalPositionsSubmitted += res.Count;
				}
				else
				{
					finalResult.FailedSubmissions++;
				}
			}
			return finalResult;
		}

		// Internal class for thread-safe result aggregation
		private class VehicleProcessResult
		{
			public bool IsSuccess { get; set; }
			public int Count { get; set; }
		}
	}
}