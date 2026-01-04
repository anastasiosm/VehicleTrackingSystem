using System.Collections.Generic;
using VehicleTracking.Core.Entities;

namespace VehicleTracking.Core.Interfaces
{
	public interface IVehicleRepository
	{
		Vehicle GetById(int id);
		IEnumerable<Vehicle> GetAll();
		IEnumerable<Vehicle> GetAllWithLastPosition();
		void Add(Vehicle vehicle);
		void Update(Vehicle vehicle);
		void SaveChanges();
	}
}