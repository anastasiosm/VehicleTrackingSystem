using VehicleTracking.Core.Models;

namespace VehicleTracking.Core.Interfaces
{
    /// <summary>
    /// Defines a contract for objects that can provide a bounding box representing their spatial extent.
    /// </summary>
    /// <remarks>Implementations of this interface should return a bounding box that accurately describes the
    /// object's boundaries in its coordinate space. This is commonly used in graphics, geometry, or spatial indexing
    /// scenarios to facilitate collision detection, rendering, or spatial queries.</remarks>
    public interface IBoundingBoxProvider
    {
        /// <summary>
        /// Returns the axis-aligned bounding box that fully contains the current object.
        /// </summary>
        /// <returns>A <see cref="BoundingBox"/> representing the smallest axis-aligned box that encloses the object.</returns>
        BoundingBox GetBoundingBox();
    }
}
