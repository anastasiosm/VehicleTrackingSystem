using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VehicleTracking.DataGenerator.Services;
using VehicleTracking.DataGenerator.Models;

namespace VehicleTracking.DataGenerator
{
	public class GpsDataGenerator
	{
		private readonly GeneratorConfig _config;
		private readonly VehicleApiClient _apiClient;
		private readonly CoordinateGenerator _coordinateGenerator;
		private readonly Random _random;

		// Athens bounding box
		private const double MIN_LAT = 37.9;
		private const double MAX_LAT = 38.1;
		private const double MIN_LON = 23.6;
		private const double MAX_LON = 23.8;

		public GpsDataGenerator(GeneratorConfig config)
		{
			_config = config;
			_apiClient = new VehicleApiClient(config.ApiBaseUrl);
			_coordinateGenerator = new CoordinateGenerator(MIN_LAT, MAX_LAT, MIN_LON, MAX_LON);
			_random = new Random();
		}

		public async Task<GenerationResult> GenerateAndSubmitPositionsAsync()
		{
			var result = new GenerationResult();

			// Step 1: Get all vehicles with their last known position
			var vehicles = await _apiClient.GetVehiclesWithLastPositionsAsync();

			if (vehicles == null || !vehicles.Any())
			{
				Console.WriteLine("  - No vehicles found in the system (API returned 0 vehicles)");
				return result;
			}

			Console.WriteLine($"  - System check: Found {vehicles.Count} vehicles in the system.");

			// Step 2: Generate and submit positions for each vehicle
			foreach (var vehicle in vehicles)
			{
				try
				{
					// Determine starting position
					double startLat, startLon;
					DateTime lastTimestamp;

					if (vehicle.LastPosition != null)
					{
						// Continue from last known position
						startLat = vehicle.LastPosition.Latitude;
						startLon = vehicle.LastPosition.Longitude;
						lastTimestamp = vehicle.LastPosition.RecordedAt;
					}
					else
					{

						// Only in the case that the vehicle is brand new and doesn't have a last position
						// Generate random starting position within Athens.
						startLat = _coordinateGenerator.GetRandomLatitude();
						startLon = _coordinateGenerator.GetRandomLongitude();
						lastTimestamp = DateTime.UtcNow.AddMinutes(-10);
					}

					// Step 3: Generate M new positions for the vehicle
					// starting from the above starting position (startLat, startLon).
					var positions = GeneratePositions(
						vehicle.Id,
						startLat,
						startLon,
						lastTimestamp,
						_config.PositionsPerVehicle,
						_config.RadiusMeters
					);

					// Step 4: Submit positions
					var success = await _apiClient.SubmitPositionsBatchAsync(vehicle.Id, positions);

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
				catch (Exception ex)
				{
					Console.WriteLine($"  - Error processing vehicle {vehicle.Name}: {ex.Message}");
					result.FailedSubmissions++;
				}
			}

			return result;
		}

		private List<GpsPositionData> GeneratePositions(
			int vehicleId,
			double startLat,
			double startLon,
			DateTime lastTimestamp,
			int positionsPerVehicle,
			double radiusMeters)
		{
			var positions = new List<GpsPositionData>();
			var currentLat = startLat;
			var currentLon = startLon;
			var currentTime = lastTimestamp;

			for (int i = 0; i < positionsPerVehicle; i++) // Generate M positions
			{
				// Generate next timestamp (2-10 seconds after previous):
				// 1. create a random increment between 2 and 10 seconds
				var secondsIncrement = _random.Next(2, 11);
				// 2. add this random number (seconds) to current time
				currentTime = currentTime.AddSeconds(secondsIncrement);

				// Generate next position (coordinates) within radius from current position
				double nextLat;
				double nextLon;
				var nextCoordinate = _coordinateGenerator.GenerateNearbyCoordinate(
					currentLat,
					currentLon,
					radiusMeters
				);
				// Extract values using Coordinate properties
				nextLat = nextCoordinate.Latitude;
				nextLon = nextCoordinate.Longitude;

				// Ensure within Athens bounding box
				if (!_coordinateGenerator.IsWithinBoundingBox(nextLat, nextLon))
				{
					// If out of bounds, generate a position closer to center
					var centerLat = (MIN_LAT + MAX_LAT) / 2;
					var centerLon = (MIN_LON + MAX_LON) / 2;

					var centerCoord = _coordinateGenerator.GenerateNearbyCoordinate(
						centerLat,
						centerLon,
						radiusMeters
					);

					// Extract values from centerCoord using Coordinate properties
					nextLat = centerCoord.Latitude;
					nextLon = centerCoord.Longitude;
				}

				positions.Add(new GpsPositionData
				{
					Latitude = nextLat,
					Longitude = nextLon,
					RecordedAt = currentTime // Stores the "new" timestamp
				});

				// Update current position for next iteration
				currentLat = nextLat;
				currentLon = nextLon;
			}

			return positions;
		}
	}
}