using System.Collections.Generic;

namespace VehicleTracking.Web.Models
{
	public class SubmitGpsPositionBatchRequest
	{
		public int VehicleId { get; set; }
		public List<GpsPositionData> Positions { get; set; }
	}
}
