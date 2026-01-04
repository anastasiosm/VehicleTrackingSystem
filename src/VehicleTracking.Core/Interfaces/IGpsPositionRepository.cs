using System;
using System.Collections.Generic;
using VehicleTracking.Core.Entities;

namespace VehicleTracking.Core.Interfaces
{
	public interface IGpsPositionRepository
	{
		void Add(GpsPosition position);
		void AddRange(IEnumerable<GpsPosition> positions);
		GpsPosition GetLastPositionForVehicle(int vehicleId);
		IEnumerable<GpsPosition> GetPositionsForVehicle(int vehicleId, DateTime from, DateTime to);
		bool PositionExists(int vehicleId, DateTime recordedAt);
		void SaveChanges();
	}
}