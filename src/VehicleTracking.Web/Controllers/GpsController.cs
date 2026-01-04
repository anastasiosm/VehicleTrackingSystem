using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AutoMapper;
using VehicleTracking.Core.Entities;
using VehicleTracking.Core.Services;
using VehicleTracking.Core.Interfaces;
using VehicleTracking.Web.Models;

namespace VehicleTracking.Web.Controllers
{
	[RoutePrefix("api/gps")]
	public class GpsController : ApiController
	{
		private readonly GpsService _gpsService;
		private readonly IVehicleRepository _vehicleRepository;
		private readonly IGpsPositionRepository _gpsPositionRepository;
		private readonly IMapper _mapper;

		public GpsController(GpsService gpsService, IVehicleRepository vehicleRepository, IGpsPositionRepository gpsPositionRepository, IMapper mapper)
		{
			_gpsService = gpsService;
			_vehicleRepository = vehicleRepository;
			_gpsPositionRepository = gpsPositionRepository;
			_mapper = mapper;
		}

		// POST api/gps/position
		[HttpPost]
		[Route("position")]
		public IHttpActionResult SubmitPosition([FromBody] SubmitGpsPositionRequest request)
		{
			if (request == null)
			{
				return BadRequest("Request cannot be null");
			}

			try
			{
				var position = _mapper.Map<GpsPosition>(request);
				_gpsService.SubmitPosition(position);

				return Ok(new ApiResponse<object>
				{
					Success = true,
					Message = "Position submitted successfully"
				});
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (Exception ex)
			{
				return InternalServerError(ex);
			}
		}

		// POST api/gps/positions/batch
		[HttpPost]
		[Route("positions/batch")]
		public IHttpActionResult SubmitPositionsBatch([FromBody] SubmitGpsPositionBatchRequest request)
		{
			if (request == null || request.Positions == null || !request.Positions.Any())
			{
				return BadRequest("Request and positions cannot be null or empty");
			}

			try
			{
				// 1. we convert the whole list of position DTOs to position entities
				var positions = _mapper.Map<List<GpsPosition>>(request.Positions);
				
				foreach (var pos in positions)
				{
					// 2. we set the VehicleId for all positions to the one in the request
					pos.VehicleId = request.VehicleId;
				}

				_gpsService.SubmitPositions(positions);

				return Ok(new ApiResponse<object>
				{
					Success = true,
					Message = $"{positions.Count} positions submitted successfully"
				});
			}
			catch (ArgumentException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(ex.Message);
			}
			catch (Exception ex)
			{
				return InternalServerError(ex);
			}
		}

		// GET api/gps/vehicle/{vehicleId}/positions
		[HttpGet]
		[Route("vehicle/{vehicleId:int}/positions")]
		public IHttpActionResult GetVehiclePositions(int vehicleId, [FromUri] DateTime? from = null, [FromUri] DateTime? to = null)
		{
			try
			{
				var vehicle = _vehicleRepository.GetById(vehicleId);
				if (vehicle == null)
				{
					return NotFound();
				}

				// Default to last 24 hours if not specified
				var fromDate = from ?? DateTime.UtcNow.AddHours(-24);
				var toDate = to ?? DateTime.UtcNow;

				var positions = _gpsPositionRepository.GetPositionsForVehicle(vehicleId, fromDate, toDate);
				var positionDtos = _mapper.Map<List<GpsPositionDto>>(positions);

				return Ok(new ApiResponse<object>
				{
					Success = true,
					Data = positionDtos
				});
			}
			catch (Exception ex)
			{
				return InternalServerError(ex);
			}
		}

		// GET api/gps/vehicle/{vehicleId}/route
		[HttpGet]
		[Route("vehicle/{vehicleId:int}/route")]
		public IHttpActionResult GetVehicleRoute(int vehicleId, [FromUri] DateTime? from = null, [FromUri] DateTime? to = null)
		{
			try
			{
				var vehicle = _vehicleRepository.GetById(vehicleId);
				if (vehicle == null)
				{
					return NotFound();
				}

				var fromDate = from ?? DateTime.UtcNow.AddHours(-24);
				var toDate = to ?? DateTime.UtcNow;

				var positions = _gpsPositionRepository.GetPositionsForVehicle(vehicleId, fromDate, toDate).ToList();

				// Calculate total distance
				double totalDistance = 0;
				for (int i = 0; i < positions.Count - 1; i++)
				{
					var current = positions[i];
					var next = positions[i + 1];

					totalDistance += GpsService.CalculateDistance(
						current.Latitude, current.Longitude,
						next.Latitude, next.Longitude
					);
				}

				var response = new VehicleRouteResponse
				{
					VehicleId = vehicleId,
					VehicleName = vehicle.Name,
					Positions = _mapper.Map<List<GpsPositionDto>>(positions),
					TotalDistanceMeters = totalDistance,
					PositionCount = positions.Count
				};

				return Ok(new ApiResponse<VehicleRouteResponse>
				{
					Success = true,
					Data = response
				});
			}
			catch (Exception ex)
			{
				return InternalServerError(ex);
			}
		}

		// GET api/gps/vehicle/{vehicleId}/last-position
		[HttpGet]
		[Route("vehicle/{vehicleId:int}/last-position")]
		public IHttpActionResult GetLastPosition(int vehicleId)
		{
			try
			{
				var vehicle = _vehicleRepository.GetById(vehicleId);
				if (vehicle == null)
				{
					return NotFound();
				}

				var lastPosition = _gpsPositionRepository.GetLastPositionForVehicle(vehicleId);

				if (lastPosition == null)
				{
					return Ok(new ApiResponse<object>
					{
						Success = true,
						Data = null,
						Message = "No positions found for this vehicle"
					});
				}

				var positionDto = _mapper.Map<GpsPositionDto>(lastPosition);

				return Ok(new ApiResponse<GpsPositionDto>
				{
					Success = true,
					Data = positionDto
				});
			}
			catch (Exception ex)
			{
				return InternalServerError(ex);
			}
		}
	}
}