using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using VehicleTracking.Core.Entities;
using VehicleTracking.Core.Interfaces;
using VehicleTracking.Data.Context;

namespace VehicleTracking.Data.Repositories
{
	public class VehicleRepository : IVehicleRepository
	{
		private readonly VehicleTrackingContext _context;

		public VehicleRepository(VehicleTrackingContext context)
		{
			_context = context;
		}

		public Vehicle GetById(int id)
		{
			return _context.Vehicles.Find(id);
		}

		public IEnumerable<Vehicle> GetAll()
		{
			return _context.Vehicles
				.OrderBy(v => v.Name)
				.ToList();
		}

		public IEnumerable<Vehicle> GetAllWithLastPosition()
		{
			// Get all vehicles with their last GPS position using a subquery approach
			var vehicles = _context.Vehicles
				.OrderBy(v => v.Name)
				.ToList();

			// Fetch last positions in a separate query for better performance
			var lastPositions = _context.GpsPositions
				.GroupBy(g => g.VehicleId)
				.Select(g => g.OrderByDescending(p => p.RecordedAt).FirstOrDefault())
				.ToList();

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

		public void SaveChanges()
		{
			_context.SaveChanges();
		}
	}
}