using VehicleTracking.Domain.ValueObjects;
using System;

namespace VehicleTracking.DataGenerator.Services
{
	public class CoordinateGenerator
	{
		private readonly double _minLat;
		private readonly double _maxLat;
		private readonly double _minLon;
		private readonly double _maxLon;
		private readonly Random _random;

		private const double EARTH_RADIUS_METERS = 6371000;

		public CoordinateGenerator(double minLat, double maxLat, double minLon, double maxLon)
		{
			_minLat = minLat;
			_maxLat = maxLat;
			_minLon = minLon;
			_maxLon = maxLon;
			_random = new Random();
		}

		public double GetRandomLatitude()
		{
			return _minLat + (_maxLat - _minLat) * _random.NextDouble();
		}

		public double GetRandomLongitude()
		{
			return _minLon + (_maxLon - _minLon) * _random.NextDouble();
		}

		public bool IsWithinBoundingBox(double lat, double lon)
		{
			return lat >= _minLat && lat <= _maxLat && lon >= _minLon && lon <= _maxLon;
		}

		/// <summary>
		/// Generate a random coordinate within a specified radius from a given point
		/// Uses a simplified approach suitable for small distances
		/// Returns a <see cref="Coordinate"/> instance instead of a tuple.
		/// </summary>
		public Coordinate GenerateNearbyCoordinate(
			double centerLat,
			double centerLon,
			double radiusMeters)
		{
			// Generate random angle (0 to 2p)
			var angle = _random.NextDouble() * 2 * Math.PI;

			// Generate random distance (0 to radius)
			// Use square root for uniform distribution
			var distance = Math.Sqrt(_random.NextDouble()) * radiusMeters;

			// Calculate offset in degrees
			// 1 degree of latitude ˜ 111,320 meters
			// 1 degree of longitude varies by latitude
			var latOffset = (distance * Math.Cos(angle)) / 111320.0;
			var lonOffset = (distance * Math.Sin(angle)) / (111320.0 * Math.Cos(ToRadians(centerLat)));

			var newLat = centerLat + latOffset;
			var newLon = centerLon + lonOffset;

			// Clamp to bounding box if needed
			newLat = Math.Max(_minLat, Math.Min(_maxLat, newLat));
			newLon = Math.Max(_minLon, Math.Min(_maxLon, newLon));

			return new Coordinate
			{
				Latitude = newLat,
				Longitude = newLon
			};
		}

		/// <summary>
		/// Calculate distance between two coordinates using Haversine formula
		/// </summary>
		public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
		{
			var dLat = ToRadians(lat2 - lat1);
			var dLon = ToRadians(lon2 - lon1);

			var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
					Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
					Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

			var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

			return EARTH_RADIUS_METERS * c;
		}

		private double ToRadians(double degrees)
		{
			return degrees * Math.PI / 180.0;
		}
	}

	public class Coordinate
	{
		public double Latitude { get; set; }
		public double Longitude { get; set; }
	}
}

