using System.Collections.Generic;

namespace VehicleTracking.Application.Dtos
{
	public class SubmitGpsPositionBatchRequest
	{
		public int VehicleId { get; set; }
		public List<GpsPositionData> Positions { get; set; }
	}
}
