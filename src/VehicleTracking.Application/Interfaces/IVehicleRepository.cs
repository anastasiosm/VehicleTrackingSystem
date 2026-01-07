using System.Collections.Generic;
using System.Threading.Tasks;
using VehicleTracking.Domain.Entities;

namespace VehicleTracking.Application.Interfaces
{
	public interface IVehicleRepository
	{
		Task<Vehicle> GetByIdAsync(int id);
		Task<IEnumerable<Vehicle>> GetAllAsync();
		Task<IEnumerable<Vehicle>> GetAllWithLastPositionAsync();
		void Add(Vehicle vehicle);
		void Update(Vehicle vehicle);
		Task SaveChangesAsync();
	}
}
