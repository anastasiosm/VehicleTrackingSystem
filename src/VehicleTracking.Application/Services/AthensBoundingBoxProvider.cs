using VehicleTracking.Domain.ValueObjects;
using VehicleTracking.Application.Interfaces;


namespace VehicleTracking.Application.Services
{
	/// <summary>
	/// Provides a bounding box representing the geographic area of Athens, Greece.
	/// </summary>
	/// <remarks>This provider returns a fixed bounding box covering the approximate latitude and longitude range of
	/// Athens. It can be used in mapping or geospatial applications that require the boundaries of Athens for filtering or
	/// display purposes.</remarks>
	public class AthensBoundingBoxProvider : IBoundingBoxProvider
	{
		/// <summary>
		/// Returns the bounding box for Athens, Greece.	
		/// </summary>
		/// <returns></returns>
		public BoundingBox GetBoundingBox()
		{
			return new BoundingBox(37.9, 38.1, 23.6, 23.8);
		}
	}	
}

