using VehicleTracking.Domain.ValueObjects;
using System.Collections.Generic;
using VehicleTracking.Domain.Entities;
using VehicleTracking.Application.Models;


namespace VehicleTracking.Application.Interfaces
{
    /// <summary>
    /// Defines methods for validating GPS position data, either individually or in batches.
    /// </summary>
    /// <remarks>Implementations of this interface can be used to verify the accuracy, plausibility, or
    /// integrity of GPS position information before further processing. The validation criteria may vary depending on
    /// the application's requirements, such as checking for valid coordinates, timestamp ranges, or signal quality.
    /// This interface is typically used in scenarios where reliable location data is critical, such as navigation,
    /// tracking, or geofencing.</remarks>
    public interface IGpsPositionValidator
    {
		/// <summary>
		/// Validates a single GPS position and returns the validation result. 
		/// </summary>
		ValidationResult ValidatePosition(GpsPosition position);

        /// <summary>
        /// Validates a batch of GPS positions and returns the overall validation result.
        /// </summary>
        /// <param name="positions">An enumerable collection of GPS positions to validate. Cannot be null. Each position in the collection is
        /// individually validated according to predefined rules.</param>
        /// <returns>A ValidationResult indicating whether the batch of positions passed validation. The result contains details
        /// about any validation failures.</returns>
        ValidationResult ValidateBatch(IEnumerable<GpsPosition> positions);
    }
}

