using System;
using System.Collections.Generic;
using System.Linq;
using VehicleTracking.Core.Entities;
using VehicleTracking.Core.Interfaces;

namespace VehicleTracking.Core.Services
{
	public class GpsService : IGpsService
	{
		private readonly IGpsPositionRepository _gpsPositionRepository;
		private readonly IVehicleRepository _vehicleRepository;

		// Athens bounding box
		private const double MIN_LAT = 37.9;
		private const double MAX_LAT = 38.1;
		private const double MIN_LON = 23.6;
		private const double MAX_LON = 23.8;

		public GpsService(IGpsPositionRepository gpsPositionRepository, IVehicleRepository vehicleRepository)
		{
			_gpsPositionRepository = gpsPositionRepository;
			_vehicleRepository = vehicleRepository;
		}

		public bool SubmitPosition(GpsPosition position)
		{
			// Validate vehicle exists and is active
			var vehicle = _vehicleRepository.GetById(position.VehicleId);
			if (vehicle == null)
			{
				throw new ArgumentException($"Vehicle with ID {position.VehicleId} does not exist.");
			}

			if (!vehicle.IsActive)
			{
				throw new InvalidOperationException($"Vehicle {vehicle.Name} is inactive and cannot accept new positions.");
			}

			// Validate coordinates within Athens bounding box
			if (!IsWithinAthensBoundingBox(position.Latitude, position.Longitude))
			{
				throw new ArgumentException($"Coordinates ({position.Latitude}, {position.Longitude}) are outside Athens bounding box.");
			}

			// Check for duplicate (VehicleId, RecordedAt)
			if (_gpsPositionRepository.PositionExists(position.VehicleId, position.RecordedAt))
			{
				throw new InvalidOperationException($"Position for vehicle {position.VehicleId} at {position.RecordedAt} already exists.");
			}

			_gpsPositionRepository.Add(position);
			_gpsPositionRepository.SaveChanges();

			return true;
		}

		public bool SubmitPositions(IEnumerable<GpsPosition> positions)
		{
			if (positions == null || !positions.Any())
			{
				throw new ArgumentException("Positions collection cannot be null or empty.");
			}

			var vehicleId = positions.First().VehicleId;

			// Validate all positions belong to the same vehicle
			if (positions.Any(p => p.VehicleId != vehicleId))
			{
				throw new ArgumentException("All positions must belong to the same vehicle.");
			}

			// Validate vehicle exists and is active
			var vehicle = _vehicleRepository.GetById(vehicleId);
			if (vehicle == null)
			{
				throw new ArgumentException($"Vehicle with ID {vehicleId} does not exist.");
			}

			if (!vehicle.IsActive)
			{
				throw new InvalidOperationException($"Vehicle {vehicle.Name} is inactive and cannot accept new positions.");
			}

			var validPositions = new List<GpsPosition>();

			foreach (var position in positions)
			{
				// Validate coordinates
				if (!IsWithinAthensBoundingBox(position.Latitude, position.Longitude))
				{
					continue; // Skip invalid positions
				}

				// Check for duplicate
				if (_gpsPositionRepository.PositionExists(position.VehicleId, position.RecordedAt))
				{
					continue; // Skip duplicates
				}

				validPositions.Add(position);
			}

			if (validPositions.Any())
			{
				_gpsPositionRepository.AddRange(validPositions);
				_gpsPositionRepository.SaveChanges();
			}

			return true;
		}

		public IEnumerable<GpsPosition> GetVehiclePositions(int vehicleId, DateTime from, DateTime to)
		{
			return _gpsPositionRepository.GetPositionsForVehicle(vehicleId, from, to);
		}

		public GpsPosition GetLastPosition(int vehicleId)
		{
			return _gpsPositionRepository.GetLastPositionForVehicle(vehicleId);
		}

		private bool IsWithinAthensBoundingBox(double latitude, double longitude)
		{
			return latitude >= MIN_LAT && latitude <= MAX_LAT &&
				   longitude >= MIN_LON && longitude <= MAX_LON;
		}

		public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
		{
			// Haversine formula
			const double R = 6371000; // Earth radius in meters

			var dLat = ToRadians(lat2 - lat1);
			var dLon = ToRadians(lon2 - lon1);

			var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
					Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
					Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

			var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

			return R * c; // Distance in meters
		}

		private static double ToRadians(double degrees)
		{
			return degrees * Math.PI / 180;
		}
	}
}