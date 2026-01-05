using VehicleTracking.Domain.ValueObjects;

namespace VehicleTracking.Application.Interfaces
{
    /// <summary>
    /// Contract for providing geographical boundary definitions (bounding boxes).
    /// </summary>
    public interface IBoundingBoxProvider
    {
        /// <summary>
        /// Retrieves the defined bounding box for the application.
        /// </summary>
        /// <returns>A BoundingBox object representing the geographical boundaries.</returns>
        BoundingBox GetBoundingBox();
    }
}
