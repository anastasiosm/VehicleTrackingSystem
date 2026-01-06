using VehicleTracking.Application.Dtos;

namespace VehicleTracking.DataGenerator.Dtos
{
	public class VehicleWithLastPositionResponse
	{
		public VehicleDto Vehicle { get; set; }
		public GpsPositionDto LastPosition { get; set; }
	}
}
