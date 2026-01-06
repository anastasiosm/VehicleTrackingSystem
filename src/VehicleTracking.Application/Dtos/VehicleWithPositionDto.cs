using System;

namespace VehicleTracking.Application.Dtos
{
	public class VehicleWithPositionDto
	{
		public VehicleDto Vehicle { get; set; }
		public GpsPositionDto LastPosition { get; set; }
	}
}
