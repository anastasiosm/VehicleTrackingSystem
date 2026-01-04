using System;
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

	public class GpsPositionRepository : IGpsPositionRepository
	{
		private readonly VehicleTrackingContext _context;

		public GpsPositionRepository(VehicleTrackingContext context)
		{
			_context = context;
		}

		public void Add(GpsPosition position)
		{
			_context.GpsPositions.Add(position);
		}

		public void AddRange(IEnumerable<GpsPosition> positions)
		{
			_context.GpsPositions.AddRange(positions);
		}

		public GpsPosition GetLastPositionForVehicle(int vehicleId)
		{
			return _context.GpsPositions
				.Where(g => g.VehicleId == vehicleId)
				.OrderByDescending(g => g.RecordedAt)
				.FirstOrDefault();
		}

		public IEnumerable<GpsPosition> GetPositionsForVehicle(int vehicleId, DateTime from, DateTime to)
		{
			return _context.GpsPositions
				.Where(g => g.VehicleId == vehicleId &&
						   g.RecordedAt >= from &&
						   g.RecordedAt <= to)
				.OrderBy(g => g.RecordedAt)
				.ToList();
		}

		public bool PositionExists(int vehicleId, DateTime recordedAt)
		{
			return _context.GpsPositions
				.Any(g => g.VehicleId == vehicleId && g.RecordedAt == recordedAt);
		}

		public void SaveChanges()
		{
			_context.SaveChanges();
		}
	}
}