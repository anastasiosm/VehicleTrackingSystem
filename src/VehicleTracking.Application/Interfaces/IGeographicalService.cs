using VehicleTracking.Domain.ValueObjects;


namespace VehicleTracking.Application.Interfaces
{
    /// <summary>
    /// Provides methods for performing geographical calculations, such as determining whether a location is within a
    /// specified boundary and calculating the distance between two coordinates.
    /// </summary>
    /// <remarks>Implementations of this interface may use different algorithms or coordinate systems. All
    /// latitude and longitude values are expected to be in decimal degrees. Thread safety and performance
    /// characteristics depend on the specific implementation.</remarks>
    public interface IGeographicalService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="boundingBox"></param>
        /// <returns></returns>
        bool IsWithinBoundary(double latitude, double longitude, BoundingBox boundingBox);

        /// <summary>
        /// Calculates the straight-line distance between two geographic coordinates.
        /// </summary>
        /// <remarks>This method uses the Haversine formula to compute the great-circle distance between
        /// two points on the Earth's surface. The calculation assumes coordinates are specified in decimal degrees and
        /// returns the shortest distance over the Earth's surface.</remarks>
        /// <param name="from">The starting location as a set of geographic coordinates. Cannot be null.</param>
        /// <param name="to">The destination location as a set of geographic coordinates. Cannot be null.</param>
        /// <returns>The distance, in kilometers, between the specified coordinates.</returns>
        double CalculateDistance(Coordinates from, Coordinates to);
    }
}

