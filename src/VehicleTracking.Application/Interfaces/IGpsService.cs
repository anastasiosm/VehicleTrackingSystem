using System;
using System.Collections.Generic;
using VehicleTracking.Domain.Entities;

namespace VehicleTracking.Application.Interfaces
{
	public interface IGpsService
	{
		bool SubmitPosition(GpsPosition position);
		bool SubmitPositions(IEnumerable<GpsPosition> positions);
		IEnumerable<GpsPosition> GetVehiclePositions(int vehicleId, DateTime from, DateTime to);
		GpsPosition GetLastPosition(int vehicleId);
	}
}
