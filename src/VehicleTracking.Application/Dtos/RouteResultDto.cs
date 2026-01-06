using System.Collections.Generic;

namespace VehicleTracking.Application.Dtos
{
	public class RouteResultDto
	{
		public int VehicleId { get; set; }
		public string VehicleName { get; set; }
		public List<GpsPositionDto> Positions { get; set; }
		public double TotalDistanceMeters { get; set; }
		public int PositionCount { get; set; }
	}
}
