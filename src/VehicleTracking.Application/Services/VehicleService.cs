using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

		public async Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync()
		{
			var vehicles = await _vehicleRepository.GetAllAsync();
			return _mapper.Map<IEnumerable<VehicleDto>>(vehicles);
		}

		public async Task<VehicleDto> GetVehicleByIdAsync(int id)
		{
			var vehicle = await _vehicleRepository.GetByIdAsync(id);
			return _mapper.Map<VehicleDto>(vehicle);
		}

		public async Task<IEnumerable<VehicleWithPositionDto>> GetVehiclesWithLastPositionsAsync()
		{
			var vehicles = await _vehicleRepository.GetAllAsync();
			var result = new List<VehicleWithPositionDto>();

			foreach (var vehicle in vehicles)
			{
				var lastPosition = await _gpsPositionRepository.GetLastPositionForVehicleAsync(vehicle.Id);
				result.Add(new VehicleWithPositionDto
				{
					Vehicle = _mapper.Map<VehicleDto>(vehicle),
					LastPosition = _mapper.Map<GpsPositionDto>(lastPosition)
				});
			}

			return result;
		}

		public async Task<VehicleWithPositionDto> GetVehicleWithLastPositionAsync(int id)
		{
			var vehicle = await _vehicleRepository.GetByIdAsync(id);
			if (vehicle == null) return null;

			var lastPosition = await _gpsPositionRepository.GetLastPositionForVehicleAsync(id);
			return new VehicleWithPositionDto
			{
				Vehicle = _mapper.Map<VehicleDto>(vehicle),
				LastPosition = _mapper.Map<GpsPositionDto>(lastPosition)
			};
		}
	}
}
