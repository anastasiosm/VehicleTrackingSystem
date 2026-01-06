using System.Collections.Generic;
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
		IEnumerable<VehicleDto> GetAllVehicles();
		VehicleDto GetVehicleById(int id);
		IEnumerable<VehicleWithPositionDto> GetVehiclesWithLastPositions();
		VehicleWithPositionDto GetVehicleWithLastPosition(int id);
	}
}
