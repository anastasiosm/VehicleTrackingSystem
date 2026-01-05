using System.Collections.Generic;
using System.Linq;
using VehicleTracking.Core.Entities;
using VehicleTracking.Core.Interfaces;

namespace VehicleTracking.Core.Services
{
	public class VehicleService : IVehicleService
	{
		private readonly IVehicleRepository _vehicleRepository;
		private readonly IGpsPositionRepository _gpsPositionRepository;

		public VehicleService(IVehicleRepository vehicleRepository, IGpsPositionRepository gpsPositionRepository)
		{
			_vehicleRepository = vehicleRepository;
			_gpsPositionRepository = gpsPositionRepository;
		}

		public IEnumerable<Vehicle> GetAllVehicles()
		{
			return _vehicleRepository.GetAll();
		}

		public Vehicle GetVehicleById(int id)
		{
			return _vehicleRepository.GetById(id);
		}

		public IEnumerable<VehicleWithPosition> GetVehiclesWithLastPositions()
		{
			var vehicles = _vehicleRepository.GetAll();
			return vehicles.Select(v => new VehicleWithPosition
			{
				Vehicle = v,
				LastPosition = _gpsPositionRepository.GetLastPositionForVehicle(v.Id)
			}).ToList();
		}

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