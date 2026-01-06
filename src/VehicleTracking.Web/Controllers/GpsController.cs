using VehicleTracking.Domain.Exceptions;
using VehicleTracking.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using AutoMapper;
using Serilog;
using VehicleTracking.Domain.Entities;
using VehicleTracking.Application.Interfaces;
using VehicleTracking.Application.Dtos;
using VehicleTracking.Web.Models;
using System.Diagnostics;

namespace VehicleTracking.Web.Controllers
{
	/// <summary>
	/// API Controller for GPS position management.
	/// Handles GPS data submission, route calculation, and position retrieval for vehicles.
	/// </summary>
	[RoutePrefix("api/gps")]
	public class GpsController : ApiController
	{
		private readonly IGpsService _gpsService;
		private readonly IMapper _mapper;
		private readonly ILogger _logger;

		private const int DEFAULT_HOURS_BACK = 24;

		public GpsController(
			IGpsService gpsService, 
			IMapper mapper,
			ILogger logger)
		{
			_gpsService = gpsService;
			_mapper = mapper;
			_logger = logger;
		}

		/// <summary>
		/// POST api/gps/position
		/// Submits a single GPS position for a vehicle.
		/// </summary>
		/// <param name="request">GPS position data including vehicle ID, coordinates, and timestamp</param>
		/// <returns>Success confirmation or validation error</returns>
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

				_logger.Information("Position submitted for vehicle {VehicleId} at {Latitude}, {Longitude}", 
					position.VehicleId, position.Latitude, position.Longitude);

				return Ok(new ApiResponse<object>
				{
					Success = true,
					Message = "Position submitted successfully"
				});
			}
			catch (ValidationException ex)
			{
				_logger.Warning("Validation failed for position submission: {Message}", ex.Message);
				return BadRequest(ex.Message);
			}
			catch (EntityNotFoundException ex)
			{
				_logger.Warning("Entity not found during position submission: {Message}", ex.Message);
				return NotFound();
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "Unexpected error during position submission");
				return InternalServerError(ex);
			}
		}

		/// <summary>
		/// POST api/gps/positions/batch
		/// Submits multiple GPS positions for a vehicle in a single request.
		/// </summary>
		/// <param name="request">Batch request containing vehicle ID and array of GPS positions</param>
		/// <returns>Success confirmation with count of submitted positions or validation error</returns>
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
				var sw = Stopwatch.StartNew();
				
				var positions = _mapper.Map<List<GpsPosition>>(request.Positions);
				
				foreach (var pos in positions)
				{
					pos.VehicleId = request.VehicleId;
				}

				_gpsService.SubmitPositions(positions);
				
				sw.Stop();
				_logger.Information("Batch of {Count} positions submitted for vehicle {VehicleId} in {ElapsedMS}ms", 
					positions.Count, request.VehicleId, sw.ElapsedMilliseconds);

				return Ok(new ApiResponse<object>
				{
					Success = true,
					Message = $"{positions.Count} positions submitted successfully"
				});
			}
			catch (ValidationException ex)
			{
				_logger.Warning("Validation failed for batch position submission: {Message}", ex.Message);
				return BadRequest(ex.Message);
			}
			catch (EntityNotFoundException ex)
			{
				_logger.Warning("Entity not found during batch position submission: {Message}", ex.Message);
				return NotFound();
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "Unexpected error during batch position submission");
				return InternalServerError(ex);
			}
		}

		/// <summary>
		/// GET api/gps/vehicle/{vehicleId}/positions
		/// Retrieves GPS positions for a specific vehicle within a date range.
		/// </summary>
		/// <param name="vehicleId">The ID of the vehicle</param>
		/// <param name="from">Start date (optional, defaults to 24 hours ago)</param>
		/// <param name="to">End date (optional, defaults to now)</param>
		/// <returns>Array of GPS positions or 404 if vehicle not found</returns>
		[HttpGet]
		[Route("vehicle/{vehicleId:int}/positions")]
		public IHttpActionResult GetVehiclePositions(int vehicleId, [FromUri] DateTime? from = null, [FromUri] DateTime? to = null)
		{
			try
			{
				// Default to last 24 hours if not specified
				var fromDate = from ?? DateTime.UtcNow.AddHours(-DEFAULT_HOURS_BACK);
				var toDate = to ?? DateTime.UtcNow;

				var routeResult = _gpsService.GetRoute(vehicleId, fromDate, toDate);

				return Ok(new ApiResponse<object>
				{
					Success = true,
					Data = routeResult.Positions
				});
			}
			catch (EntityNotFoundException ex)
			{
				_logger.Warning("Vehicle with ID {VehicleId} not found", vehicleId);
				return NotFound();
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "Error retrieving positions for vehicle {VehicleId}", vehicleId);
				return InternalServerError(ex);
			}
		}

		/// <summary>
		/// GET api/gps/vehicle/{vehicleId}/route
		/// Calculates and retrieves the complete route for a vehicle including distance calculations.
		/// </summary>
		/// <param name="vehicleId">The ID of the vehicle</param>
		/// <param name="from">Start date (optional, defaults to 24 hours ago)</param>
		/// <param name="to">End date (optional, defaults to now)</param>
		/// <returns>Route data with positions, total distance, and vehicle information or 404 if vehicle not found</returns>
		[HttpGet]
		[Route("vehicle/{vehicleId:int}/route")]
		public IHttpActionResult GetVehicleRoute(int vehicleId, [FromUri] DateTime? from = null, [FromUri] DateTime? to = null)
		{
			try
			{
				var fromDate = from ?? DateTime.UtcNow.AddHours(-DEFAULT_HOURS_BACK);
				var toDate = to ?? DateTime.UtcNow;

				var routeResult = _gpsService.GetRoute(vehicleId, fromDate, toDate);
				var response = _mapper.Map<VehicleRouteResponse>(routeResult);

				return Ok(new ApiResponse<VehicleRouteResponse>
				{
					Success = true,
					Data = response
				});
			}
			catch (EntityNotFoundException ex)
			{
				_logger.Warning("Vehicle with ID {VehicleId} not found", vehicleId);
				return NotFound();
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "Error calculating route for vehicle {VehicleId}", vehicleId);
				return InternalServerError(ex);
			}
		}

		/// <summary>
		/// GET api/gps/vehicle/{vehicleId}/last-position
		/// Retrieves the most recent GPS position for a specific vehicle.
		/// </summary>
		/// <param name="vehicleId">The ID of the vehicle</param>
		/// <returns>Last known GPS position or 404 if vehicle not found</returns>
		[HttpGet]
		[Route("vehicle/{vehicleId:int}/last-position")]
		public IHttpActionResult GetLastPosition(int vehicleId)
		{
			try
			{
				var lastPosition = _gpsService.GetLastPosition(vehicleId);

				return Ok(new ApiResponse<GpsPositionDto>
				{
					Success = true,
					Data = lastPosition,
					Message = lastPosition == null ? "No positions found for this vehicle" : null
				});
			}
			catch (EntityNotFoundException ex)
			{
				_logger.Warning("Vehicle with ID {VehicleId} not found", vehicleId);
				return NotFound();
			}
			catch (ValidationException ex)
			{
				_logger.Warning("Validation failed for GetLastPosition: {Message}", ex.Message);
				return BadRequest(ex.Message);
			}
			catch (Exception ex)
			{
				_logger.Error(ex, "Error retrieving last position for vehicle {VehicleId}", vehicleId);
				return InternalServerError(ex);
			}
		}
	}
}
