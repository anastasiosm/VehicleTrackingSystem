using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using VehicleTracking.Domain.Entities;
using VehicleTracking.Application.Interfaces;
using VehicleTracking.Persistence.Context;

namespace VehicleTracking.Persistence.Repositories
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

		public async Task<GpsPosition> GetLastPositionForVehicleAsync(int vehicleId)
		{
			return await _context.GpsPositions
				.Where(g => g.VehicleId == vehicleId)
				.OrderByDescending(g => g.RecordedAt)
				.FirstOrDefaultAsync();
		}

		public async Task<IEnumerable<GpsPosition>> GetPositionsForVehicleAsync(int vehicleId, DateTime from, DateTime to)
		{
			return await _context.GpsPositions
				.Where(g => g.VehicleId == vehicleId &&
						   g.RecordedAt >= from &&
						   g.RecordedAt <= to)
				.OrderBy(g => g.RecordedAt)
				.ToListAsync();
		}

		public async Task<bool> PositionExistsAsync(int vehicleId, DateTime recordedAt)
		{
			return await _context.GpsPositions
				.AnyAsync(g => g.VehicleId == vehicleId && g.RecordedAt == recordedAt);
		}

		public async Task SaveChangesAsync()
		{
			await _context.SaveChangesAsync();
		}
	}
}
