using System;

namespace VehicleTracking.Web.Models
{
	public class VehicleListDto
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public bool IsActive { get; set; }
		public DateTime? LastPositionTimestamp { get; set; }
		public double? LastLatitude { get; set; }
		public double? LastLongitude { get; set; }
	}
}
