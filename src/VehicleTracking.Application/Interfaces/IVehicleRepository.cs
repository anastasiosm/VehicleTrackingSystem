using System.Collections.Generic;
using VehicleTracking.Domain.Entities;

namespace VehicleTracking.Application.Interfaces
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
