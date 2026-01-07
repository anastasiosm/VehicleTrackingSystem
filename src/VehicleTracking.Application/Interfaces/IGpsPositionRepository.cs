using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VehicleTracking.Domain.Entities;

namespace VehicleTracking.Application.Interfaces
{
	public interface IGpsPositionRepository
	{
		void Add(GpsPosition position);
		void AddRange(IEnumerable<GpsPosition> positions);
		Task<GpsPosition> GetLastPositionForVehicleAsync(int vehicleId);
		Task<IEnumerable<GpsPosition>> GetPositionsForVehicleAsync(int vehicleId, DateTime from, DateTime to);
		Task<bool> PositionExistsAsync(int vehicleId, DateTime recordedAt);
		Task SaveChangesAsync();
	}
}
