using System;
using System.Collections.Generic;
using VehicleTracking.Application.Interfaces;
using VehicleTracking.DataGenerator.Models;

namespace VehicleTracking.DataGenerator.Services
{
	public class AthensPositionSimulator : IPositionSimulator
	{
		private readonly CoordinateGenerator _coordinateGenerator;
		private readonly IBoundingBoxProvider _boundingBoxProvider;
		private readonly Random _random = new Random();

		public AthensPositionSimulator(IBoundingBoxProvider boundingBoxProvider)
		{
			_boundingBoxProvider = boundingBoxProvider;
			var box = _boundingBoxProvider.GetBoundingBox();
			_coordinateGenerator = new CoordinateGenerator(
				box.MinLatitude, 
				box.MaxLatitude, 
				box.MinLongitude, 
				box.MaxLongitude);
		}

		public List<GpsPositionData> GeneratePath(GpsPositionData startPoint, int count, double radiusMeters)
		{
			var positions = new List<GpsPositionData>();
			var currentLat = startPoint.Latitude;
			var currentLon = startPoint.Longitude;
			var currentTime = startPoint.RecordedAt;
			var box = _boundingBoxProvider.GetBoundingBox();

			for (int i = 0; i < count; i++)
			{
				currentTime = currentTime.AddSeconds(_random.Next(2, 11));
				var nextCoord = _coordinateGenerator.GenerateNearbyCoordinate(currentLat, currentLon, radiusMeters);

				if (!_coordinateGenerator.IsWithinBoundingBox(nextCoord.Latitude, nextCoord.Longitude))
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
