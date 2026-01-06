using System;

namespace VehicleTracking.Application.Dtos
{
	public class GpsPositionData
	{
		public double Latitude { get; set; }
		public double Longitude { get; set; }
		public DateTime RecordedAt { get; set; }
	}
}
