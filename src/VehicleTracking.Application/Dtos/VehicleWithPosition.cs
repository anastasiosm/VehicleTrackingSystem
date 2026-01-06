using VehicleTracking.Domain.Entities;

namespace VehicleTracking.Application.Dtos
{
	/// <summary>
	/// Represents a vehicle combined with its last known GPS position.
	/// This is a composite model used for queries that need both vehicle information and location data.
	/// </summary>
	public class VehicleWithPosition
	{
		/// <summary>
		/// Gets or sets the vehicle entity.
		/// </summary>
		public Vehicle Vehicle { get; set; }

		/// <summary>
		/// Gets or sets the last known GPS position for this vehicle.
		/// Can be null if the vehicle has no recorded positions yet.
		/// </summary>
		public GpsPosition LastPosition { get; set; }
	}
}
