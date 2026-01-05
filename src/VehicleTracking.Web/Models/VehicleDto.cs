using System;

namespace VehicleTracking.Web.Models
{
	// Vehicle DTOs
	public class VehicleDto
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public bool IsActive { get; set; }
		public DateTime CreatedDate { get; set; }
		public GpsPositionDto LastKnownPosition { get; set; }
	}
}
