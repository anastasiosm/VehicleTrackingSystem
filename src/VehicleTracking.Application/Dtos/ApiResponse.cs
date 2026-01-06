using System.Collections.Generic;

namespace VehicleTracking.Application.Dtos
{
	public class ApiResponse<T>
	{
		public ApiResponse()
		{
			Errors = new List<string>();
		}

		public bool Success { get; set; }
		public T Data { get; set; }
		public string Message { get; set; }
		public List<string> Errors { get; set; }
	}
}
