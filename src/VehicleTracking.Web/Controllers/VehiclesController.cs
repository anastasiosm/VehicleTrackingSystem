using VehicleTracking.Domain.ValueObjects;
using System;
using System.Linq;
using System.Web.Http;
using Serilog;
using VehicleTracking.Application.Interfaces;
using VehicleTracking.Application.Models;
using VehicleTracking.Web.Services;

namespace VehicleTracking.Web.Controllers
{
	/// <summary>
	/// API Controller for managing vehicles.
	/// Provides endpoints for retrieving vehicle information and their last known positions.
	/// </summary>
	[RoutePrefix("api/vehicles")]
	public class VehiclesController : ApiController
	{
		private readonly IVehicleService _vehicleService;
		private readonly IVehiclePositionMapper _vehiclePositionMapper;
		private readonly IApiResponseBuilder _responseBuilder;
		private readonly ILogger _logger;

		public VehiclesController(
			IVehicleService vehicleService,
			IVehiclePositionMapper vehiclePositionMapper,
			IApiResponseBuilder responseBuilder,
			ILogger logger)
		{
			_vehicleService = vehicleService;
			_vehiclePositionMapper = vehiclePositionMapper;
			_responseBuilder = responseBuilder;
			_logger = logger;
		}

		/// <summary>
		/// GET api/vehicles
		/// Retrieves all vehicles with basic information and last position timestamp/coordinates.
		/// </summary>
		[HttpGet]
		[Route("")]
		public IHttpActionResult GetAllVehicles()
		{
			try
			{
				var vehiclesWithPositions = _vehicleService.GetVehiclesWithLastPositions();
				var vehicleDtos = vehiclesWithPositions
					.Select(vp => _vehiclePositionMapper.MapToListDto(vp))
					.ToList();

				return Ok(_responseBuilder.Success(vehicleDtos));
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "Error retrieving all vehicles");
				return InternalServerError(ex);
			}
		}

		/// <summary>
		/// GET api/vehicles/{id}
		/// Retrieves detailed information about a specific vehicle including its last known position.
		/// </summary>
		[HttpGet]
		[Route("{id:int}")]
		public IHttpActionResult GetVehicle(int id)
		{
			try
			{
				var vehicleWithPosition = _vehicleService.GetVehicleWithLastPosition(id);

				if (vehicleWithPosition?.Vehicle == null)
				{
					_logger.Warning("Vehicle with ID {VehicleId} not found", id);
					return NotFound();
				}

				var vehicleDto = _vehiclePositionMapper.MapToDetailDto(vehicleWithPosition);

				return Ok(_responseBuilder.Success(vehicleDto));
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "Error retrieving vehicle with ID {VehicleId}", id);
				return InternalServerError(ex);
			}
		}

		/// <summary>
		/// GET api/vehicles/with-last-positions
		/// Retrieves all vehicles with complete last position details.
		/// Returns vehicle and position as separate objects in the response.
		/// </summary>
		[HttpGet]
		[Route("with-last-positions")]
		public IHttpActionResult GetVehiclesWithLastPositions()
		{
			try
			{
				var vehiclesWithPositions = _vehicleService.GetVehiclesWithLastPositions();

				var result = vehiclesWithPositions.Select(vp =>
				{
					var vehicleDto = _vehiclePositionMapper.MapToListDto(vp);
					var lastPositionDto = vp.LastPosition != null 
						? new
						{
							id = vp.LastPosition.Id,
							vehicleId = vp.LastPosition.VehicleId,
							latitude = vp.LastPosition.Latitude,
							longitude = vp.LastPosition.Longitude,
							recordedAt = vp.LastPosition.RecordedAt
						}
						: null;

					return new
					{
						vehicle = vehicleDto,
						lastPosition = lastPositionDto
					};
				}).ToList();

				return Ok(_responseBuilder.Success(result));
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "Error retrieving vehicles with last positions");
				return InternalServerError(ex);
			}
		}
	}
}

