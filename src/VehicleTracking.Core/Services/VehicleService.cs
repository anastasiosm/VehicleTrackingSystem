using System.Collections.Generic;
using System.Linq;
using VehicleTracking.Core.Entities;
using VehicleTracking.Core.Interfaces;
using VehicleTracking.Core.Models;

namespace VehicleTracking.Core.Services
{
	/// <summary>
	/// Implementation of vehicle service providing business logic for vehicle operations.
	/// Orchestrates between vehicle and GPS position repositories.
	/// </summary>
	public class VehicleService : IVehicleService
	{
		private readonly IVehicleRepository _vehicleRepository;
		private readonly IGpsPositionRepository _gpsPositionRepository;

		public VehicleService(IVehicleRepository vehicleRepository, IGpsPositionRepository gpsPositionRepository)
		{
			_vehicleRepository = vehicleRepository;
			_gpsPositionRepository = gpsPositionRepository;
		}

		/// <summary>
		/// Retrieves all vehicles from the repository.
		/// </summary>
		public IEnumerable<Vehicle> GetAllVehicles()
		{
			return _vehicleRepository.GetAll();
		}

		/// <summary>
		/// Retrieves a vehicle by its ID.
		/// </summary>
		public Vehicle GetVehicleById(int id)
		{
			return _vehicleRepository.GetById(id);
		}

		/// <summary>
		/// Retrieves all vehicles with their last GPS positions.
		/// Efficiently combines vehicle data with position data in a single operation.
		/// </summary>
		public IEnumerable<VehicleWithPosition> GetVehiclesWithLastPositions()
		{
			var vehicles = _vehicleRepository.GetAll();
			return vehicles.Select(v => new VehicleWithPosition
			{
				Vehicle = v,
				LastPosition = _gpsPositionRepository.GetLastPositionForVehicle(v.Id)
			}).ToList();
		}

		/// <summary>
		/// Retrieves a specific vehicle with its last GPS position.
		/// Returns null if the vehicle is not found.
		/// </summary>
		public VehicleWithPosition GetVehicleWithLastPosition(int id)
		{
			var vehicle = _vehicleRepository.GetById(id);
			if (vehicle == null) return null;

			return new VehicleWithPosition
			{
				Vehicle = vehicle,
				LastPosition = _gpsPositionRepository.GetLastPositionForVehicle(id)
			};
		}
	}
}