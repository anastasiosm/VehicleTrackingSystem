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
					var lastPos = _gpsPositionRepository.GetLastPositionForVehicle(v.Id);
					return new
					{
						vehicle = new VehicleListDto
						{
							Id = v.Id,
							Name = v.Name,
							IsActive = v.IsActive,
							LastPositionTimestamp = lastPos?.RecordedAt,
							LastLatitude = lastPos?.Latitude,
							LastLongitude = lastPos?.Longitude
						},
						lastPosition = lastPos != null ? new GpsPositionDto
						{
							Id = lastPos.Id,
							VehicleId = lastPos.VehicleId,
							Latitude = lastPos.Latitude,
							Longitude = lastPos.Longitude,
							RecordedAt = lastPos.RecordedAt
						} : null
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