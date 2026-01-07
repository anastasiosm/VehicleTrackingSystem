using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VehicleTracking.Domain.Entities;

using VehicleTracking.Application.Dtos;

namespace VehicleTracking.Application.Interfaces
{
	public interface IGpsService
	{
		Task<bool> SubmitPositionAsync(GpsPosition position);
		Task<bool> SubmitPositionsAsync(IEnumerable<GpsPosition> positions);
		Task<RouteResultDto> GetRouteAsync(int vehicleId, DateTime from, DateTime to);
		Task<GpsPositionDto> GetLastPositionAsync(int vehicleId);
	}
}
