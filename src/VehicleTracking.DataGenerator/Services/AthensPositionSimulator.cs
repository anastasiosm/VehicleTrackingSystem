using System;
using System.Collections.Generic;
using VehicleTracking.Application.Interfaces;
using VehicleTracking.Application.Dtos;
using VehicleTracking.DataGenerator.Dtos;
using VehicleTracking.Infrastructure.Services; // add reference to Infrastructure services
using VehicleTracking.Domain.ValueObjects; // add reference for Coordinates

namespace VehicleTracking.DataGenerator.Services
{
	public class AthensPositionSimulator : IPositionSimulator
	{
		private readonly CoordinateGenerator _coordinateGenerator; // use Infrastructure CoordinateGenerator
		private readonly IBoundingBoxProvider _boundingBoxProvider;
		private readonly IGeographicalService _geographicalService; // add geographical service
		
		private const int MIN_INTERVAL_SECONDS = 2; // minimum time between position updates
		private const int MAX_INTERVAL_SECONDS = 11; // maximum time between position updates

		public AthensPositionSimulator(
			IBoundingBoxProvider boundingBoxProvider, 
			IGeographicalService geographicalService) // inject geographical service
		{
			_boundingBoxProvider = boundingBoxProvider;
			_geographicalService = geographicalService;
			var box = _boundingBoxProvider.GetBoundingBox();
			_coordinateGenerator = new CoordinateGenerator(box, geographicalService); // use BoundingBox struct and service
		}

		public GpsPositionData GetDefaultStartingPoint()
		{
			var box = _boundingBoxProvider.GetBoundingBox();
			return new GpsPositionData
			{
				Latitude = (box.MinLatitude + box.MaxLatitude) / 2,
				Longitude = (box.MinLongitude + box.MaxLongitude) / 2,
				RecordedAt = DateTime.UtcNow.AddMinutes(-10)
			};
		}

		public List<GpsPositionData> GeneratePath(GpsPositionData startPoint, int count, double radiusMeters)
		{
			var positions = new List<GpsPositionData>();
			var currentLat = startPoint.Latitude;
			var currentLon = startPoint.Longitude;
			
			// Start from the next timestamp after the last position
			// If the last position is old, start from current time
			var lastRecordedTime = startPoint.RecordedAt;
			var currentTime = lastRecordedTime > DateTime.UtcNow.AddMinutes(-5) 
				? lastRecordedTime.AddSeconds(ThreadSafeRandom.Next(MIN_INTERVAL_SECONDS, MAX_INTERVAL_SECONDS))
				: DateTime.UtcNow;
			
			var box = _boundingBoxProvider.GetBoundingBox();

			for (int i = 0; i < count; i++)
			{
				// Use thread-safe random for interval calculation
				currentTime = currentTime.AddSeconds(ThreadSafeRandom.Next(MIN_INTERVAL_SECONDS, MAX_INTERVAL_SECONDS));
				var nextCoord = _coordinateGenerator.GenerateNearbyCoordinate(currentLat, currentLon, radiusMeters);

				// Use GeographicalService for boundary checking instead of duplicate logic
				if (!_geographicalService.IsWithinBoundary(nextCoord.Latitude, nextCoord.Longitude, box))
				{
					// If out of bounds, return closer to center
					nextCoord = _coordinateGenerator.GenerateNearbyCoordinate(
						(box.MinLatitude + box.MaxLatitude) / 2,
						(box.MinLongitude + box.MaxLongitude) / 2,
						radiusMeters);
				}

				positions.Add(new GpsPositionData { 
					Latitude = nextCoord.Latitude, 
					Longitude = nextCoord.Longitude, 
					RecordedAt = currentTime 
				});

				currentLat = nextCoord.Latitude;
				currentLon = nextCoord.Longitude;
			}
			return positions;
		}
	}
}