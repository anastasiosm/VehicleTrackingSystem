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
		private readonly IVehicleService _vehicleService;
		private readonly IMapper _mapper;

		public VehiclesController(IVehicleService vehicleService, IMapper mapper)
		{
			_vehicleService = vehicleService;
			_mapper = mapper;
		}

		// GET api/vehicles
		[HttpGet]
		[Route("")]
		public IHttpActionResult GetAllVehicles()
		{
			try
			{
				var vehiclesWithPositions = _vehicleService.GetVehiclesWithLastPositions();
				var vehicleDtos = vehiclesWithPositions.Select(vp =>
				{
					var dto = _mapper.Map<VehicleListDto>(vp.Vehicle);
					if (vp.LastPosition != null)
					{
						dto.LastPositionTimestamp = vp.LastPosition.RecordedAt;
						dto.LastLatitude = vp.LastPosition.Latitude;
						dto.LastLongitude = vp.LastPosition.Longitude;
					}
					return dto;
				}).ToList();

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
				var vp = _vehicleService.GetVehicleWithLastPosition(id);

				if (vp?.Vehicle == null)
				{
					return NotFound();
				}

				var vehicleDto = _mapper.Map<VehicleDto>(vp.Vehicle);
				if (vp.LastPosition != null)
				{
					vehicleDto.LastKnownPosition = _mapper.Map<GpsPositionDto>(vp.LastPosition);
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
				var vehiclesWithPositions = _vehicleService.GetVehiclesWithLastPositions();

				var result = vehiclesWithPositions.Select(vp =>
				{
					var vehicleDto = _mapper.Map<VehicleListDto>(vp.Vehicle);
					var lastPositionDto = _mapper.Map<GpsPositionDto>(vp.LastPosition);

					if (vp.LastPosition != null)
					{
						vehicleDto.LastPositionTimestamp = vp.LastPosition.RecordedAt;
						vehicleDto.LastLatitude = vp.LastPosition.Latitude;
						vehicleDto.LastLongitude = vp.LastPosition.Longitude;
					}

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