using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Serilog;
using VehicleTracking.Application.Interfaces;
using VehicleTracking.Application.Dtos;
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
		private readonly ILogger _logger;

		public VehiclesController(
			IVehicleService vehicleService,
			ILogger logger)
		{
			_vehicleService = vehicleService;
			_logger = logger;
		}

		/// <summary>
		/// GET api/vehicles
		/// Retrieves all vehicles with basic information.
		/// </summary>
		[HttpGet]
		[Route("")]
		public async Task<IHttpActionResult> GetAllVehicles()
		{
			try
			{
				var vehicleDtos = await _vehicleService.GetAllVehiclesAsync();

				return Ok(new ApiResponse<IEnumerable<VehicleDto>>
				{
					Success = true,
					Data = vehicleDtos
				});
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "Error retrieving all vehicles");
				return InternalServerError(ex);
			}
		}

		/// <summary>
		/// GET api/vehicles/{id}
		/// Retrieves detailed information about a specific vehicle.
		/// </summary>
		[HttpGet]
		[Route("{id:int}")]
		public async Task<IHttpActionResult> GetVehicle(int id)
		{
			try
			{
				var vehicleDto = await _vehicleService.GetVehicleByIdAsync(id);

				if (vehicleDto == null)
				{
					_logger.Warning("Vehicle with ID {VehicleId} not found", id);
					return NotFound();
				}

				return Ok(new ApiResponse<VehicleDto>
				{
					Success = true,
					Data = vehicleDto
				});
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
		/// </summary>
		[HttpGet]
		[Route("with-last-positions")]
		public async Task<IHttpActionResult> GetVehiclesWithLastPositions()
		{
			try
			{
				var result = await _vehicleService.GetVehiclesWithLastPositionsAsync();

				return Ok(new ApiResponse<IEnumerable<VehicleWithPositionDto>>
				{
					Success = true,
					Data = result
				});
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "Error retrieving vehicles with last positions");
				return InternalServerError(ex);
			}
		}
	}
}

