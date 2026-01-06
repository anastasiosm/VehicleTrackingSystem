using System; // import System namespace for Random and Math
using VehicleTracking.Domain.ValueObjects; // import domain value objects (Coordinates, BoundingBox)
using VehicleTracking.Application.Interfaces; // import application interfaces

namespace VehicleTracking.Infrastructure.Services // correct namespace for infrastructure
{
	/// <summary>
	/// Service for generating random and nearby geographical coordinates within specified boundaries.
	/// Used by the GPS simulator to create realistic movement patterns for vehicles.
	/// </summary>
	/// <remarks>This class handles the core logic for random coordinate generation.
	/// For geographical calculations (distance, boundary checks), it delegates to IGeographicalService.</remarks>
	public class CoordinateGenerator // class that produces random and nearby coordinates
	{
		private readonly BoundingBox _boundingBox; // geographic boundaries for coordinate generation
		private readonly IGeographicalService _geoService; // service for geographical calculations

		private const double EARTH_RADIUS_METERS = 6371000; // Earth's radius in meters for distance calculations

		/// <summary>
		/// Initializes a new instance of the CoordinateGenerator with specific geographical boundaries.
		/// </summary>
		/// <param name="boundingBox">The bounding box defining the geographical area for coordinate generation</param>
		/// <param name="geoService">Service for performing geographical calculations</param>
		/// <exception cref="ArgumentException">Thrown when boundingBox is empty/default</exception>
		/// <exception cref="ArgumentNullException">Thrown when geoService is null</exception>
		public CoordinateGenerator(BoundingBox boundingBox, IGeographicalService geoService) // constructor receiving dependencies
		{
			// Validate that bounding box is not default/empty (all zeros indicates invalid bounding box)
			if (boundingBox.MinLatitude == 0 && boundingBox.MaxLatitude == 0 &&
				boundingBox.MinLongitude == 0 && boundingBox.MaxLongitude == 0)
				throw new ArgumentException("BoundingBox cannot be empty or default", nameof(boundingBox));

			_boundingBox = boundingBox; // store validated bounding box
			_geoService = geoService ?? throw new ArgumentNullException(nameof(geoService)); // store and validate geo service
		}

		/// <summary>
		/// Generates a random latitude within the generator's defined boundaries.
		/// </summary>
		/// <returns>A latitude value in decimal degrees</returns>
		public double GetRandomLatitude() // return a random latitude inside bounding box
		{
			return _boundingBox.MinLatitude +
				   (_boundingBox.MaxLatitude - _boundingBox.MinLatitude) * ThreadSafeRandom.NextDouble(); // compute min + range * fraction
		}

		/// <summary>
		/// Generates a random longitude within the generator's defined boundaries.
		/// </summary>
		/// <returns>A longitude value in decimal degrees</returns>
		public double GetRandomLongitude() // return a random longitude inside bounding box
		{
			return _boundingBox.MinLongitude +
				   (_boundingBox.MaxLongitude - _boundingBox.MinLongitude) * ThreadSafeRandom.NextDouble(); // compute min + range * fraction
		}

		/// <summary>
		/// Generates a random coordinate within the configured bounding box.
		/// </summary>
		/// <returns>A new Coordinates instance with random latitude and longitude values</returns>
		public Coordinates GetRandomCoordinate() // generate a complete random coordinate
		{
			return new Coordinates(GetRandomLatitude(), GetRandomLongitude()); // combine random lat and lon
		}

		/// <summary>
		/// Generates a new coordinate within a specific radius of a starting point.
		/// Useful for simulating movement where each new point is close to the previous one.
		/// </summary>
		/// <param name="startLat">Starting latitude</param>
		/// <param name="startLon">Starting longitude</param>
		/// <param name="radiusMeters">Maximum distance from start point in meters</param>
		/// <returns>A new set of Coordinates within the specified radius</returns>
		public Coordinates GenerateNearbyCoordinate(double startLat, double startLon, double radiusMeters) // produce a nearby coordinate using great-circle calculations
		{
			// Pick a random distance up to radiusMeters
			// Use square root for uniform distribution in circular area
			double distance = Math.Sqrt(ThreadSafeRandom.NextDouble()) * radiusMeters; // distance in meters between 0 and radiusMeters
			// Pick a random angle (0 to 360 degrees)
			double bearing = ThreadSafeRandom.NextDouble() * 2 * Math.PI; // bearing in radians between 0 and 2*PI

			// Convert latitude and longitude to radians
			double lat1 = ToRadians(startLat); // convert start latitude to radians
			double lon1 = ToRadians(startLon); // convert start longitude to radians

			// Calculate new position using great-circle formula
			// This accounts for the curvature of the Earth
			double lat2 = Math.Asin(Math.Sin(lat1) * Math.Cos(distance / EARTH_RADIUS_METERS) +
									Math.Cos(lat1) * Math.Sin(distance / EARTH_RADIUS_METERS) * Math.Cos(bearing)); // compute new latitude in radians

			double lon2 = lon1 + Math.Atan2(Math.Sin(bearing) * Math.Sin(distance / EARTH_RADIUS_METERS) * Math.Cos(lat1),
											Math.Cos(distance / EARTH_RADIUS_METERS) - Math.Sin(lat1) * Math.Sin(lat2)); // compute new longitude in radians

			// Convert to degrees
			double newLat = ToDegrees(lat2); // convert latitude back to degrees
			double newLon = ToDegrees(lon2); // convert longitude back to degrees

			// Clamp to bounding box to avoid producing coordinates outside defined area
			newLat = Math.Max(_boundingBox.MinLatitude, Math.Min(_boundingBox.MaxLatitude, newLat));
			newLon = Math.Max(_boundingBox.MinLongitude, Math.Min(_boundingBox.MaxLongitude, newLon));

			return new Coordinates(newLat, newLon); // return clamped coordinates
		}

		/// <summary>
		/// Generates a new coordinate within a specific radius of a starting coordinate.
		/// Useful for simulating movement where each new point is close to the previous one.
		/// </summary>
		/// <param name="start">Starting coordinates</param>
		/// <param name="radiusMeters">Maximum distance from start point in meters</param>
		/// <returns>A new set of Coordinates within the specified radius</returns>
		public Coordinates GenerateNearbyCoordinate(Coordinates start, double radiusMeters) // overload accepting Coordinates object
		{
			return GenerateNearbyCoordinate(start.Latitude, start.Longitude, radiusMeters); // delegate to main implementation
		}

		private static double ToRadians(double degrees) // helper: convert degrees to radians
		{
			return degrees * Math.PI / 180; // multiply degrees by PI/180
		}

		private static double ToDegrees(double radians) // helper: convert radians to degrees
		{
			return radians * 180 / Math.PI; // multiply radians by 180/PI
		}
	}
}