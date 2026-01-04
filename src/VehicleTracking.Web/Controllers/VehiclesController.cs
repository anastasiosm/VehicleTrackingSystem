using System;
using System.Linq;
using System.Web.Http;
using VehicleTracking.Core.Interfaces;
using VehicleTracking.Web.Models;

namespace VehicleTracking.Web.Controllers
{
	[RoutePrefix("api/vehicles")]
	public class VehiclesController : ApiController
	{
		private readonly IVehicleRepository _vehicleRepository;
		private readonly IGpsPositionRepository _gpsPositionRepository;

		public VehiclesController(IVehicleRepository vehicleRepository, IGpsPositionRepository gpsPositionRepository)
		{
			_vehicleRepository = vehicleRepository;
			_gpsPositionRepository = gpsPositionRepository;
		}

		// GET api/vehicles
		[HttpGet]
		[Route("")]
		public IHttpActionResult GetAllVehicles()
		{
			try
			{
				var vehicles = _vehicleRepository.GetAll();

				var vehicleDtos = vehicles.Select(v =>
				{
					var lastPosition = _gpsPositionRepository.GetLastPositionForVehicle(v.Id);

					return new VehicleListDto
					{
						Id = v.Id,
						Name = v.Name,
						IsActive = v.IsActive,
						LastPositionTimestamp = lastPosition?.RecordedAt,
						LastLatitude = lastPosition?.Latitude,
						LastLongitude = lastPosition?.Longitude
					};
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
				var vehicle = _vehicleRepository.GetById(id);

				if (vehicle == null)
				{
					return NotFound();
				}

				var lastPosition = _gpsPositionRepository.GetLastPositionForVehicle(id);

				var vehicleDto = new VehicleDto
				{
					Id = vehicle.Id,
					Name = vehicle.Name,
					IsActive = vehicle.IsActive,
					CreatedDate = vehicle.CreatedDate,
					LastKnownPosition = lastPosition != null ? new GpsPositionDto
					{
						Id = lastPosition.Id,
						VehicleId = lastPosition.VehicleId,
						Latitude = lastPosition.Latitude,
						Longitude = lastPosition.Longitude,
						RecordedAt = lastPosition.RecordedAt
					} : null
				};

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