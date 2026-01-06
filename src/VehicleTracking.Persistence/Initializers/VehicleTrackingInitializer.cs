using System;
using System.Collections.Generic;
using System.Data.Entity;
using VehicleTracking.Domain.Entities;
using VehicleTracking.Persistence.Context;

namespace VehicleTracking.Persistence.Initializers
{
	/// <summary>
	/// Initializes the database for the vehicle tracking system.
	/// Uses DropCreateDatabaseIfModelChanges strategy to ensure the database schema matches the model.
	/// </summary>
	/// <remarks>This initializer is intended for development and testing scenarios to provide a pre-populated
	/// database with sample vehicles and GPS positions. It creates 100 active vehicles and assigns initial GPS positions
	/// to a subset of them. The seeding logic is executed when the database is created/recreated. For production
	/// environments, consider using a different initializer or migration strategy to avoid overwriting existing
	/// data.</remarks>
	public class VehicleTrackingInitializer : DropCreateDatabaseIfModelChanges<VehicleTrackingContext>
	{
		protected override void Seed(VehicleTrackingContext context)
		{
			// Seed 100 vehicles
			var vehicles = new List<Vehicle>();
			var random = new Random();

			// Vehicle name prefixes for variety
			var prefixes = new[] { "VAN", "TRUCK", "CAR", "BUS", "TAXI", "DELIVERY", "FLEET" };

			for (int i = 1; i <= 100; i++)
			{
				var prefix = prefixes[random.Next(prefixes.Length)];
				var vehicle = new Vehicle
				{
					Name = $"{prefix}-{i:D3}",
					IsActive = true,
					CreatedDate = DateTime.UtcNow.AddDays(-random.Next(1, 365))
				};

				vehicles.Add(vehicle);
			}

			context.Vehicles.AddRange(vehicles);
			context.SaveChanges();

			// Optionally seed some initial GPS positions for a few vehicles
			// This gives us data to work with immediately
			var initialPositions = new List<GpsPosition>();
			var centerLat = 37.9838; // Syntagma Square
			var centerLon = 23.7275;

			for (int i = 0; i < 10; i++) // First 10 vehicles get initial positions
			{
				var vehicleId = vehicles[i].Id;
				var baseTime = DateTime.UtcNow.AddHours(-2);

				for (int j = 0; j < 5; j++)
				{
					var position = new GpsPosition
					{
						VehicleId = vehicleId,
						Latitude = centerLat + (random.NextDouble() - 0.5) * 0.02,
						Longitude = centerLon + (random.NextDouble() - 0.5) * 0.02,
						RecordedAt = baseTime.AddMinutes(j * 5)
					};
					initialPositions.Add(position);
				}
			}

			context.GpsPositions.AddRange(initialPositions);
			context.SaveChanges();

			base.Seed(context);
		}
	}
}
