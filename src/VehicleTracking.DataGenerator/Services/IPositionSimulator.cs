using System.Collections.Generic;
using VehicleTracking.DataGenerator.Models;

namespace VehicleTracking.DataGenerator.Services
{
	public interface IPositionSimulator
	{
		List<GpsPositionData> GeneratePath(GpsPositionData startPoint, int count, double radiusMeters);
	}
}
