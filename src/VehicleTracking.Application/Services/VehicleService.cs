using System.Collections.Generic;
using System.Linq;
using VehicleTracking.Domain.Entities;
using VehicleTracking.Application.Interfaces;
using VehicleTracking.Application.Dtos;
using AutoMapper;

namespace VehicleTracking.Application.Services
{
	/// <summary>
	/// Implementation of vehicle service providing business logic for vehicle operations.
	/// Orchestrates between vehicle and GPS position repositories.
	/// </summary>
	public class VehicleService : IVehicleService
	{
		private readonly IVehicleRepository _vehicleRepository;
		private readonly IGpsPositionRepository _gpsPositionRepository;
		private readonly IMapper _mapper;

		public VehicleService(
			IVehicleRepository vehicleRepository, 
			IGpsPositionRepository gpsPositionRepository,
			IMapper mapper)
		{
			_vehicleRepository = vehicleRepository;
			_gpsPositionRepository = gpsPositionRepository;
			_mapper = mapper;
		}

		public IEnumerable<VehicleDto> GetAllVehicles()
		{
			var vehicles = _vehicleRepository.GetAll();
			return _mapper.Map<IEnumerable<VehicleDto>>(vehicles);
		}

		public VehicleDto GetVehicleById(int id)
		{
			var vehicle = _vehicleRepository.GetById(id);
			return _mapper.Map<VehicleDto>(vehicle);
		}

		public IEnumerable<VehicleWithPositionDto> GetVehiclesWithLastPositions()
		{
			var vehicles = _vehicleRepository.GetAll();
			return vehicles.Select(v => new VehicleWithPositionDto
			{
				Vehicle = _mapper.Map<VehicleDto>(v),
				LastPosition = _mapper.Map<GpsPositionDto>(_gpsPositionRepository.GetLastPositionForVehicle(v.Id))
			}).ToList();
		}

		public VehicleWithPositionDto GetVehicleWithLastPosition(int id)
		{
			var vehicle = _vehicleRepository.GetById(id);
			if (vehicle == null) return null;

			return new VehicleWithPositionDto
			{
				Vehicle = _mapper.Map<VehicleDto>(vehicle),
				LastPosition = _mapper.Map<GpsPositionDto>(_gpsPositionRepository.GetLastPositionForVehicle(id))
			};
		}
	}
}
