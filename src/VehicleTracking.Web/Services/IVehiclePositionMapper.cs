using VehicleTracking.Application.Dtos;
using VehicleTracking.Web.Models;

namespace VehicleTracking.Web.Services
{
	/// <summary>
	/// Interface for mapping vehicle and position data to DTOs.
	/// Centralizes mapping logic to follow the Single Responsibility Principle.
	/// </summary>
	public interface IVehiclePositionMapper
	{
		/// <summary>
		/// Maps a VehicleWithPosition to a VehicleListDto for list views.
		/// Includes last position timestamp and coordinates if available.
		/// </summary>
		/// <param name="vehicleWithPosition">The vehicle with its last position</param>
		/// <returns>A DTO suitable for list displays</returns>
		VehicleListDto MapToListDto(VehicleWithPosition vehicleWithPosition);

		/// <summary>
		/// Maps a VehicleWithPosition to a detailed VehicleDto.
		/// Includes full position details if available.
		/// </summary>
		/// <param name="vehicleWithPosition">The vehicle with its last position</param>
		/// <returns>A detailed DTO with full vehicle and position information</returns>
		VehicleDto MapToDetailDto(VehicleWithPosition vehicleWithPosition);
	}
}