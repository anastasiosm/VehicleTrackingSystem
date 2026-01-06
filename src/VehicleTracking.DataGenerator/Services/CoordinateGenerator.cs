using VehicleTracking.Domain.ValueObjects;
using System;

namespace VehicleTracking.DataGenerator.Services
{
	/// <summary>
	/// Provides methods for generating random geographic coordinates and performing spatial calculations within a
	/// specified latitude and longitude bounding box.
	/// </summary>
	/// <remarks>Use this class to simulate or test geographic data by generating random coordinates within defined
	/// bounds, checking if coordinates are inside the bounding box, generating nearby coordinates within a radius, and
	/// calculating distances between points. All generated coordinates and calculations respect the configured latitude
	/// and longitude limits. This class is suitable for applications such as mapping, geospatial analysis, and
	/// location-based testing.</remarks>
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

		/// <summary>
		/// Generates a random latitude value within the configured minimum and maximum latitude bounds.
		/// </summary>
		/// <remarks>This method is useful for simulating geographic coordinates or generating test data within a
		/// specific latitude range. The returned value is uniformly distributed between the minimum and maximum latitude
		/// values set for the instance.</remarks>
		/// <returns>A double representing a randomly selected latitude. The value will be greater than or equal to the minimum
		/// latitude and less than or equal to the maximum latitude.</returns>
		public double GetRandomLatitude()
		{
			return _minLat + (_maxLat - _minLat) * _random.NextDouble();
		}

		/// <summary>
		/// Generates a random longitude value within the configured minimum and maximum longitude range.
		/// </summary>
		/// <returns>A double representing a randomly selected longitude value between the minimum and maximum bounds.</returns>
		public double GetRandomLongitude()
		{
			return _minLon + (_maxLon - _minLon) * _random.NextDouble();
		}

		/// <summary>
		/// Determines whether the specified latitude and longitude coordinates are within the bounding box defined by this
		/// instance.
		/// </summary>
		/// <param name="lat">The latitude value to test, in decimal degrees. Must be within the valid range for geographic coordinates.</param>
		/// <param name="lon">The longitude value to test, in decimal degrees. Must be within the valid range for geographic coordinates.</param>
		/// <returns>true if the specified coordinates are inside the bounding box; otherwise, false.</returns>
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

