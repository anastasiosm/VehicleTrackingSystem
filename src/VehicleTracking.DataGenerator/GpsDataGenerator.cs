using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using VehicleTracking.Application.Interfaces;
using VehicleTracking.DataGenerator.Models;
using VehicleTracking.DataGenerator.Services;

namespace VehicleTracking.DataGenerator
{
	public class GpsDataGenerator
	{
		private readonly GeneratorConfig _config;
		private readonly IVehicleApiClient _apiClient;
		private readonly IPositionSimulator _simulator;
		private readonly IBoundingBoxProvider _boundingBoxProvider;
		private readonly ILogger _logger;

		public GpsDataGenerator(
			GeneratorConfig config, 
			IBoundingBoxProvider boundingBoxProvider,
			IVehicleApiClient apiClient,
			IPositionSimulator simulator,
			ILogger logger)
		{
			_config = config;
			_boundingBoxProvider = boundingBoxProvider;
			_apiClient = apiClient;
			_simulator = simulator;
			_logger = logger;
		}

		public async Task<GenerationResult> GenerateAndSubmitPositionsAsync()
		{
			var result = new GenerationResult();
			// Step 1: Get all vehicles with their last known position
			var vehicles = await _apiClient.GetVehiclesWithLastPositionsAsync();

			if (vehicles == null || !vehicles.Any())
			{
				_logger.Warning("No vehicles found in the system (API returned 0 vehicles)");
				return result;
			}

			_logger.Information("System check: Found {Count} vehicles in the system. Starting parallel processing...", vehicles.Count);

			// Step 2: Generate and submit positions for each vehicle
			// Parallel processing of all vehicles
			var tasks = vehicles.Select(async vehicle =>
			{
				try
				{
					var startPoint = GetStartingPoint(vehicle);
					var positions = _simulator.GeneratePath(startPoint, _config.PositionsPerVehicle, _config.RadiusMeters);

					var success = await _apiClient.SubmitPositionsBatchAsync(vehicle.Id, positions);

					lock (result) // Thread-safe update of shared result object
					{
						if (success)
						{
							result.VehiclesProcessed++;
							result.TotalPositionsSubmitted += positions.Count;
						}
						else
						{
							result.FailedSubmissions++;
						}
					}
				}
				catch (Exception ex)
				{
					_logger.Error(ex, "Error processing vehicle {VehicleName} (ID: {VehicleId})", vehicle.Name, vehicle.Id);
					lock (result) { result.FailedSubmissions++; }
				}
			});

			await Task.WhenAll(tasks);
			return result;
		}

		private GpsPositionData GetStartingPoint(VehicleWithLastPosition vehicle)
		{
			if (vehicle.LastPosition != null)
			{
				return new GpsPositionData
				{
					Latitude = vehicle.LastPosition.Latitude,
					Longitude = vehicle.LastPosition.Longitude,
					RecordedAt = vehicle.LastPosition.RecordedAt
				};
			}

			// For new vehicles, start from Athens center
			var box = _boundingBoxProvider.GetBoundingBox();
			return new GpsPositionData
			{
				Latitude = (box.MinLatitude + box.MaxLatitude) / 2,
				Longitude = (box.MinLongitude + box.MaxLongitude) / 2,
				RecordedAt = DateTime.UtcNow.AddMinutes(-10)
			};
		}
	}
}
