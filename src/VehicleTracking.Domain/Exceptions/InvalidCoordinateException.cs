using System;

namespace VehicleTracking.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when coordinates are outside valid geographical ranges.
    /// This is a domain rule: latitude must be [-90, 90], longitude must be [-180, 180].
    /// </summary>
    public class InvalidCoordinateException : DomainException
    {
        public double Latitude { get; }
        public double Longitude { get; }

        public InvalidCoordinateException(double latitude, double longitude)
            : base($"Invalid coordinates: Latitude={latitude}, Longitude={longitude}. " +
                   "Latitude must be between -90 and 90, Longitude must be between -180 and 180.")
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}
