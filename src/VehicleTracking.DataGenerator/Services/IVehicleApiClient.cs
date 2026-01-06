using System.Collections.Generic;
using System.Threading.Tasks;
using VehicleTracking.DataGenerator.Models;

namespace VehicleTracking.DataGenerator.Services
{
	public interface IVehicleApiClient
	{
		Task<List<VehicleWithLastPosition>> GetVehiclesWithLastPositionsAsync();
		Task<bool> SubmitPositionsBatchAsync(int vehicleId, List<GpsPositionData> positions);
	}
}
