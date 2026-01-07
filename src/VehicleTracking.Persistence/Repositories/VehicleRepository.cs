using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using VehicleTracking.Domain.Entities;
using VehicleTracking.Application.Interfaces;
using VehicleTracking.Persistence.Context;

namespace VehicleTracking.Persistence.Repositories
{
	public class VehicleRepository : IVehicleRepository
	{
		private readonly VehicleTrackingContext _context;

		public VehicleRepository(VehicleTrackingContext context)
		{
			_context = context;
		}

		public async Task<Vehicle> GetByIdAsync(int id)
		{
			return await _context.Vehicles.FindAsync(id);
		}

		public async Task<IEnumerable<Vehicle>> GetAllAsync()
		{
			return await _context.Vehicles
				.OrderBy(v => v.Name)
				.ToListAsync();
		}

		public async Task<IEnumerable<Vehicle>> GetAllWithLastPositionAsync()
		{
			// Get all vehicles with their last GPS position using a subquery approach
			var vehicles = await _context.Vehicles
				.OrderBy(v => v.Name)
				.ToListAsync();

			// Fetch last positions in a separate query for better performance
			var lastPositions = await _context.GpsPositions
				.GroupBy(g => g.VehicleId)
				.Select(g => g.OrderByDescending(p => p.RecordedAt).FirstOrDefault())
				.ToListAsync();

			// Attach last position to each vehicle
			foreach (var vehicle in vehicles)
			{
				var lastPos = lastPositions.FirstOrDefault(p => p.VehicleId == vehicle.Id);
				if (lastPos != null)
				{
					vehicle.GpsPositions.Add(lastPos);
				}
			}

			return vehicles;
		}

		public void Add(Vehicle vehicle)
		{
			_context.Vehicles.Add(vehicle);
		}

		public void Update(Vehicle vehicle)
		{
			_context.Entry(vehicle).State = EntityState.Modified;
		}

		public async Task SaveChangesAsync()
		{
			await _context.SaveChangesAsync();
		}
	}
}
