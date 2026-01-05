using System;

namespace VehicleTracking.Web.Models
{
	public class SubmitGpsPositionRequest
	{
		public int VehicleId { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }
		public DateTime RecordedAt { get; set; }
	}
}
