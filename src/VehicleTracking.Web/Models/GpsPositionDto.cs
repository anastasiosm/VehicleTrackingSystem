using System;

namespace VehicleTracking.Web.Models
{
	// GPS Position DTOs
	public class GpsPositionDto
	{
		public long Id { get; set; }
		public int VehicleId { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }
		public DateTime RecordedAt { get; set; }
	}
}