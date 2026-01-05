using System.Collections.Generic;
using VehicleTracking.Domain.Entities;
using VehicleTracking.Application.Models;

namespace VehicleTracking.Application.Interfaces
{
	/// <summary>
	/// Service interface for managing vehicles and their positions.
	/// Provides read-only access to vehicle data.
	/// </summary>
	public interface IVehicleService
	{
		/// <summary>
		/// Retrieves all vehicles from the system.
		/// </summary>
		/// <returns>A collection of all vehicles</returns>
		IEnumerable<Vehicle> GetAllVehicles();

		/// <summary>
		/// Retrieves a specific vehicle by its ID.
		/// </summary>
		/// <param name="id">The unique identifier of the vehicle</param>
		/// <returns>The vehicle if found; otherwise, null</returns>
		Vehicle GetVehicleById(int id);

		/// <summary>
		/// Retrieves all vehicles along with their last known GPS positions.
		/// This is useful for dashboard views showing vehicle locations.
		/// </summary>
		/// <returns>A collection of vehicles with their last positions</returns>
		IEnumerable<VehicleWithPosition> GetVehiclesWithLastPositions();

		/// <summary>
		/// Retrieves a specific vehicle along with its last known GPS position.
		/// </summary>
		/// <param name="id">The unique identifier of the vehicle</param>
		/// <returns>The vehicle with its last position if found; otherwise, null</returns>
		VehicleWithPosition GetVehicleWithLastPosition(int id);
	}
}
