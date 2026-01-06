using System.Collections.Generic;
using VehicleTracking.Application.Dtos;
using VehicleTracking.DataGenerator.Dtos;

namespace VehicleTracking.DataGenerator.Services
{
	public interface IPositionSimulator
	{
		List<GpsPositionData> GeneratePath(GpsPositionData startPoint, int count, double radiusMeters);
		GpsPositionData GetDefaultStartingPoint();
	}
}