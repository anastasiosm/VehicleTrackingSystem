using System.Collections.Generic;
using VehicleTracking.Application.Dtos;
using VehicleTracking.Domain.Entities;

namespace VehicleTracking.Application.Interfaces
{
    /// <summary>
    /// Service for calculating routes and route statistics from GPS positions.
    /// Separated from GpsService to follow Single Responsibility Principle.
    /// </summary>
    public interface IRouteCalculationService
    {
        /// <summary>
        /// Calculates the total distance traveled based on a sequence of GPS positions.
        /// Uses great-circle distance calculation between consecutive points.
        /// </summary>
        /// <param name="positions">Ordered list of GPS positions (should be in chronological order)</param>
        /// <returns>Total distance in meters</returns>
        double CalculateTotalDistance(IEnumerable<GpsPosition> positions);

        /// <summary>
        /// Calculates comprehensive route statistics including distance, duration, and average speed.
        /// </summary>
        /// <param name="positions">Ordered list of GPS positions</param>
        /// <returns>Route statistics object with calculated metrics</returns>
        RouteStatistics CalculateRouteStatistics(IEnumerable<GpsPosition> positions);
    }
}
