using System;

namespace VehicleTracking.Application.Dtos
{
	public class VehicleDto
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public bool IsActive { get; set; }
		public DateTime CreatedDate { get; set; }
		public GpsPositionDto LastKnownPosition { get; set; }
		
		// Helper properties for flat list view
		public DateTime? LastPositionTimestamp { get; set; }
		public double? LastLatitude { get; set; }
		public double? LastLongitude { get; set; }
	}
}
