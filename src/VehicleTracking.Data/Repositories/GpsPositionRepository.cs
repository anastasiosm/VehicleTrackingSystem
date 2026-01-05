using System;
using System.Collections.Generic;
using System.Linq;
using VehicleTracking.Domain.Entities;
using VehicleTracking.Application.Interfaces;
using VehicleTracking.Data.Context;

namespace VehicleTracking.Data.Repositories
{
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
