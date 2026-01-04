using System;
using System.Linq;
using System.Web.Http;
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

		public GpsController(GpsService gpsService, IVehicleRepository vehicleRepository, IGpsPositionRepository gpsPositionRepository)
		{
			_gpsService = gpsService;
			_vehicleRepository = vehicleRepository;
			_gpsPositionRepository = gpsPositionRepository;
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
				var position = new GpsPosition
				{
					VehicleId = request.VehicleId,
					Latitude = request.Latitude,
					Longitude = request.Longitude,
					RecordedAt = request.RecordedAt
				};

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
				var positions = request.Positions.Select(p => new GpsPosition
				{
					VehicleId = request.VehicleId,
					Latitude = p.Latitude,
					Longitude = p.Longitude,
					RecordedAt = p.RecordedAt
				}).ToList();

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

				var positionDtos = positions.Select(p => new GpsPositionDto
				{
					Id = p.Id,
					VehicleId = p.VehicleId,
					Latitude = p.Latitude,
					Longitude = p.Longitude,
					RecordedAt = p.RecordedAt
				}).ToList();

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
					Positions = positions.Select(p => new GpsPositionDto
					{
						Id = p.Id,
						VehicleId = p.VehicleId,
						Latitude = p.Latitude,
						Longitude = p.Longitude,
						RecordedAt = p.RecordedAt
					}).ToList(),
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

				var positionDto = new GpsPositionDto
				{
					Id = lastPosition.Id,
					VehicleId = lastPosition.VehicleId,
					Latitude = lastPosition.Latitude,
					Longitude = lastPosition.Longitude,
					RecordedAt = lastPosition.RecordedAt
				};

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