using System;

namespace VehicleTracking.Web.Models
{
	// Query DTOs
	public class VehiclePositionsRequest
	{
		public int VehicleId { get; set; }
		public DateTime From { get; set; }
		public DateTime To { get; set; }
	}
}
