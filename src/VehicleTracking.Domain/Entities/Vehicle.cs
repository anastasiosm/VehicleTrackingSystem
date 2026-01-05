using System;
using System.Collections.Generic;

namespace VehicleTracking.Domain.Entities
{
	public class Vehicle
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public bool IsActive { get; set; }

		public DateTime CreatedDate { get; set; }

		// Navigation property
		public virtual ICollection<GpsPosition> GpsPositions { get; set; }

		public Vehicle()
		{
			GpsPositions = new HashSet<GpsPosition>();
			IsActive = true;
			CreatedDate = DateTime.UtcNow;
		}
	}
}
