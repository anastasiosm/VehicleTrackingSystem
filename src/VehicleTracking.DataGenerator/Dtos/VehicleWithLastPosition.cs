using VehicleTracking.Application.Dtos;

namespace VehicleTracking.DataGenerator.Dtos
{
	public class VehicleWithLastPosition
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public bool IsActive { get; set; }
		public GpsPositionDto LastPosition { get; set; }
	}
}
