using System;

namespace VehicleTracking.Core.Entities
{
	public class GpsPosition
	{
		public long Id { get; set; }

		public int VehicleId { get; set; }

		public double Latitude { get; set; }

		public double Longitude { get; set; }

		public DateTime RecordedAt { get; set; }

		// Navigation property
		public virtual Vehicle Vehicle { get; set; }
	}
}