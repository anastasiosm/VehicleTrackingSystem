using System.Collections.Generic;
using VehicleTracking.Core.Entities;

namespace VehicleTracking.Core.Interfaces
{
	public interface IVehicleService
	{
		IEnumerable<Vehicle> GetAllVehicles();
		Vehicle GetVehicleById(int id);
		IEnumerable<VehicleWithPosition> GetVehiclesWithLastPositions();
		VehicleWithPosition GetVehicleWithLastPosition(int id);
	}

	public class VehicleWithPosition
	{
		public Vehicle Vehicle { get; set; }
		public GpsPosition LastPosition { get; set; }
	}
}