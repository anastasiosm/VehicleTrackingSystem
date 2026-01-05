using System;
using VehicleTracking.Core.Interfaces;
using VehicleTracking.Core.Models;

namespace VehicleTracking.Core.Services
{
    /// <summary>
    /// Provides geographical calculation services, including boundary checks and distance computations between
    /// coordinates.
    /// </summary>
    /// <remarks>This class offers methods for determining whether a location falls within a specified
    /// bounding box and for calculating the distance between two geographical points using the Haversine formula. All
    /// methods assume coordinates are specified in decimal degrees. Instances of this class are thread-safe for
    /// concurrent use.</remarks>
    public class GeographicalService : IGeographicalService
    {
       /// <summary>
       /// Determines whether the specified latitude and longitude coordinates are contained within the given bounding
       /// box.
       /// </summary>
       /// <param name="latitude">The latitude value to evaluate, in degrees. Must be within the valid range for geographic coordinates.</param>
       /// <param name="longitude">The longitude value to evaluate, in degrees. Must be within the valid range for geographic coordinates.</param>
       /// <param name="boundingBox">The bounding box that defines the geographic area to check against. Must not be null.</param>
       /// <returns>true if the latitude and longitude are both within the boundaries of the specified bounding box; otherwise,
       /// false.</returns>
        public bool IsWithinBoundary(double latitude, double longitude, BoundingBox boundingBox)
        {
            return latitude >= boundingBox.MinLat && latitude <= boundingBox.MaxLat &&
                   longitude >= boundingBox.MinLon && longitude <= boundingBox.MaxLon;
        }

        /// <summary>
        /// Calculates the geodesic distance, in meters, between two geographic coordinates using the Haversine formula.
        /// </summary>
        /// <remarks>This method assumes the Earth is a perfect sphere with a mean radius of 6,371,000
        /// meters. For most applications, this provides sufficient accuracy, but may not account for local terrain or
        /// ellipsoidal effects.</remarks>
        /// <param name="from">The starting geographic coordinates. Must not be null.</param>
        /// <param name="to">The destination geographic coordinates. Must not be null.</param>
        /// <returns>The distance, in meters, between the specified coordinates. Returns 0 if the coordinates are identical.</returns>
        public double CalculateDistance(Coordinates from, Coordinates to)
        {
            const double R = 6371000;

            var dLat = ToRadians(to.Latitude - from.Latitude);
            var dLon = ToRadians(to.Longitude - from.Longitude);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(from.Latitude)) * Math.Cos(ToRadians(to.Latitude)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        /// <summary>
        /// Converts an angle measured in degrees to its equivalent in radians.
        /// </summary>
        /// <param name="degrees">The angle, in degrees, to convert to radians.</param>
        /// <returns>The angle in radians that corresponds to the specified number of degrees.</returns>
        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
}
