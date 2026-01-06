using VehicleTracking.Domain.ValueObjects;
using AutoMapper;
using VehicleTracking.Application.Dtos;
using VehicleTracking.Web.Models;

namespace VehicleTracking.Web.Services
{
	/// <summary>
	/// Implementation of vehicle position mapper.
	/// Centralizes the mapping logic between domain models and DTOs.
	/// This follows the Single Responsibility Principle by separating mapping concerns from controller logic.
	/// </summary>
	public class VehiclePositionMapper : IVehiclePositionMapper
	{
		private readonly IMapper _mapper;

		public VehiclePositionMapper(IMapper mapper)
		{
			_mapper = mapper;
		}

		/// <summary>
		/// Maps a VehicleWithPosition to a VehicleListDto.
		/// Extracts key position data (timestamp, coordinates) into the list DTO for efficient display.
		/// </summary>
		public VehicleListDto MapToListDto(VehicleWithPosition vehicleWithPosition)
		{
			// Map the base vehicle properties
			var dto = _mapper.Map<VehicleListDto>(vehicleWithPosition.Vehicle);

			// Add last position data if available
			if (vehicleWithPosition.LastPosition != null)
			{
				dto.LastPositionTimestamp = vehicleWithPosition.LastPosition.RecordedAt;
				dto.LastLatitude = vehicleWithPosition.LastPosition.Latitude;
				dto.LastLongitude = vehicleWithPosition.LastPosition.Longitude;
			}

			return dto;
		}

		/// <summary>
		/// Maps a VehicleWithPosition to a detailed VehicleDto.
		/// Includes the full GpsPositionDto object for detailed views.
		/// </summary>
		public VehicleDto MapToDetailDto(VehicleWithPosition vehicleWithPosition)
		{
			// Map the base vehicle properties
			var vehicleDto = _mapper.Map<VehicleDto>(vehicleWithPosition.Vehicle);

			// Map the complete last position if available
			if (vehicleWithPosition.LastPosition != null)
			{
				vehicleDto.LastKnownPosition = _mapper.Map<GpsPositionDto>(vehicleWithPosition.LastPosition);
			}

			return vehicleDto;
		}
	}
}

