using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VehicleTracking.DataGenerator.Services;
using VehicleTracking.DataGenerator.Models;

namespace VehicleTracking.DataGenerator
{
	/*
		PSEUDOCODE / PLAN (detailed):

		Goal: Remove tuple deconstruction usage from GeneratePositions method while keeping behavior identical.

		Steps:
		1. For each generated position iteration:
			a. Compute a random seconds increment and add to currentTime.
			b. Request a nearby coordinate from _coordinateGenerator given currentLat, currentLon, and radiusMeters.
			c. Store the returned value in a local variable (e.g. nextCoord).
			d. Extract latitude and longitude from nextCoord using explicit properties (.Item1, .Item2)
			   and assign them to local doubles nextLat and nextLon.
		2. Validate that nextLat and nextLon are within the Athens bounding box using IsWithinBoundingBox:
			a. If inside, continue.
			b. If outside, compute centerLat and centerLon as the midpoint of bounding box.
			c. Request another nearby coordinate around the center (store in centerCoord).
			d. Extract values from centerCoord into nextLat and nextLon using .Item1/.Item2.
		3. Create a new GpsPositionData with nextLat, nextLon, and currentTime and add to positions list.
		4. Update currentLat and currentLon to nextLat/nextLon for the next loop iteration.

		Notes:
		- Avoid tuple deconstruction syntax like 'var (a,b) = ...'.
		- Use explicit local variables and access tuple elements via Item1/Item2 instead.
		- Keep existing randomness, timestamp progression, and bounding checks unchanged.
	*/

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
				Console.WriteLine("  ⚠ No vehicles found in the system (API returned 0 vehicles)");
				return result;
			}

			Console.WriteLine($"  → System check: Found {vehicles.Count} vehicles in the system.");

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
						// Generate random starting position within Athens
						startLat = _coordinateGenerator.GetRandomLatitude();
						startLon = _coordinateGenerator.GetRandomLongitude();
						lastTimestamp = DateTime.UtcNow.AddMinutes(-10);
					}

					// Step 3: Generate M new positions
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
					Console.WriteLine($"  ⚠ Error processing vehicle {vehicle.Name}: {ex.Message}");
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
			int count,
			double radiusMeters)
		{
			var positions = new List<GpsPositionData>();
			var currentLat = startLat;
			var currentLon = startLon;
			var currentTime = lastTimestamp;

			for (int i = 0; i < count; i++)
			{
				// Generate next timestamp (2-10 seconds after previous)
				var secondsIncrement = _random.Next(2, 11);
				currentTime = currentTime.AddSeconds(secondsIncrement);

				// Generate next position within radius from current position
				double nextLat;
				double nextLon;
				var nextCoord = _coordinateGenerator.GenerateNearbyCoordinate(
					currentLat,
					currentLon,
					radiusMeters
				);
				// Extract values using Coordinate properties
				nextLat = nextCoord.Latitude;
				nextLon = nextCoord.Longitude;

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
					RecordedAt = currentTime
				});

				// Update current position for next iteration
				currentLat = nextLat;
				currentLon = nextLon;
			}

			return positions;
		}
	}
}