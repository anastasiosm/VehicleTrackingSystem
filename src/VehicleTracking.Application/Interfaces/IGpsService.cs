using System;
using System.Collections.Generic;
using VehicleTracking.Domain.Entities;

using VehicleTracking.Application.Dtos;

namespace VehicleTracking.Application.Interfaces
{
	public interface IGpsService
	{
		bool SubmitPosition(GpsPosition position);
		bool SubmitPositions(IEnumerable<GpsPosition> positions);
		RouteResultDto GetRoute(int vehicleId, DateTime from, DateTime to);
		GpsPositionDto GetLastPosition(int vehicleId);
	}
}
