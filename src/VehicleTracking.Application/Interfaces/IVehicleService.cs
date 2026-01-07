using System.Collections.Generic;
using System.Threading.Tasks;
using VehicleTracking.Domain.Entities;
using VehicleTracking.Application.Dtos;

namespace VehicleTracking.Application.Interfaces
{
	/// <summary>
	/// Service interface for managing vehicles and their positions.
	/// Provides read-only access to vehicle data.
	/// </summary>
	public interface IVehicleService
	{
		Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync();
		Task<VehicleDto> GetVehicleByIdAsync(int id);
		Task<IEnumerable<VehicleWithPositionDto>> GetVehiclesWithLastPositionsAsync();
		Task<VehicleWithPositionDto> GetVehicleWithLastPositionAsync(int id);
	}
}
