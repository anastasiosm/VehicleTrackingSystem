using System;

namespace VehicleTracking.Web.Models
{
	public class GpsPositionData
	{
		public double Latitude { get; set; }
		public double Longitude { get; set; }
		public DateTime RecordedAt { get; set; }
	}
}
