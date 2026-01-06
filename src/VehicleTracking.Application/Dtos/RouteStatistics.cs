namespace VehicleTracking.Application.Dtos
{
    /// <summary>
    /// Statistical data calculated from a route (sequence of GPS positions).
    /// </summary>
    public class RouteStatistics
    {
        /// <summary>
        /// Total distance traveled in meters.
        /// </summary>
        public double TotalDistanceMeters { get; set; }

        /// <summary>
        /// Number of GPS positions in the route.
        /// </summary>
        public int PositionCount { get; set; }

        /// <summary>
        /// Duration of the route in seconds.
        /// </summary>
        public double DurationSeconds { get; set; }

        /// <summary>
        /// Average speed calculated as distance/duration (meters per second).
        /// </summary>
        public double AverageSpeedMetersPerSecond { get; set; }
    }
}
