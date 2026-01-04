using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AutoMapper;
using VehicleTracking.Core.Interfaces;
using VehicleTracking.Web.Models;

namespace VehicleTracking.Web.Controllers
{
	[RoutePrefix("api/vehicles")]
	public class VehiclesController : ApiController
	{
		private readonly IVehicleRepository _vehicleRepository;
		private readonly IGpsPositionRepository _gpsPositionRepository;
		private readonly IMapper _mapper;

		public VehiclesController(IVehicleRepository vehicleRepository, IGpsPositionRepository gpsPositionRepository, IMapper mapper)
		{
			_vehicleRepository = vehicleRepository;
			_gpsPositionRepository = gpsPositionRepository;
			_mapper = mapper;
		}

		// GET api/vehicles
		[HttpGet]
		[Route("")]
		public IHttpActionResult GetAllVehicles()
		{
			try
			{
				var vehicles = _vehicleRepository.GetAll();
				var vehicleDtos = _mapper.Map<List<VehicleListDto>>(vehicles);

				foreach (var dto in vehicleDtos)
				{
					var lastPosition = _gpsPositionRepository.GetLastPositionForVehicle(dto.Id);
					if (lastPosition != null)
					{
						dto.LastPositionTimestamp = lastPosition.RecordedAt;
						dto.LastLatitude = lastPosition.Latitude;
						dto.LastLongitude = lastPosition.Longitude;
					}
				}

				return Ok(new ApiResponse<object>
				{
					Success = true,
					Data = vehicleDtos
				});
			}
			catch (Exception ex)
			{
				return InternalServerError(ex);
			}
		}

		// GET api/vehicles/{id}
		[HttpGet]
		[Route("{id:int}")]
		public IHttpActionResult GetVehicle(int id)
		{
			try
			{
				var vehicle = _vehicleRepository.GetById(id);

				if (vehicle == null)
				{
					return NotFound();
				}

				var vehicleDto = _mapper.Map<VehicleDto>(vehicle);
				var lastPosition = _gpsPositionRepository.GetLastPositionForVehicle(id);

				if (lastPosition != null)
				{
					vehicleDto.LastKnownPosition = _mapper.Map<GpsPositionDto>(lastPosition);
				}

				return Ok(new ApiResponse<VehicleDto>
				{
					Success = true,
					Data = vehicleDto
				});
			}
			catch (Exception ex)
			{
				return InternalServerError(ex);
			}
		}

		// GET api/vehicles/with-last-positions
		[HttpGet]
		[Route("with-last-positions")]
		public IHttpActionResult GetVehiclesWithLastPositions()
		{
			try
			{
				var vehicles = _vehicleRepository.GetAll();

				var result = vehicles.Select(v =>
				{
					// Fetch the latest GPS position for the current vehicle from the database
					var lastPos = _gpsPositionRepository.GetLastPositionForVehicle(v.Id);

					// Map the Vehicle entity to a VehicleListDto using AutoMapper
					var vehicleDto = _mapper.Map<VehicleListDto>(v);

					// Map the GpsPosition entity to a GpsPositionDto
					var lastPositionDto = _mapper.Map<GpsPositionDto>(lastPos);

					if (lastPos != null)
					{
						// Enrich the vehicle DTO with flattened data from the last known position
						vehicleDto.LastPositionTimestamp = lastPos.RecordedAt;
						vehicleDto.LastLatitude = lastPos.Latitude;
						vehicleDto.LastLongitude = lastPos.Longitude;
					}

					// Return a combined anonymous object containing both DTOs
					return new
					{
						vehicle = vehicleDto,
						lastPosition = lastPositionDto
					};
				}).ToList();

				return Ok(new ApiResponse<object>
				{
					Success = true,
					Data = result
				});
			}
			catch (Exception ex)
			{
				return InternalServerError(ex);
			}
		}
	}
}