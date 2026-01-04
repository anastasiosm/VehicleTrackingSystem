using System;
using System.Collections.Generic;

namespace VehicleTracking.DataGenerator.Models
{
	// Vehicle models
	public class VehicleDto
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public bool IsActive { get; set; }
		public DateTime? LastPositionTimestamp { get; set; }
		public double? LastLatitude { get; set; }
		public double? LastLongitude { get; set; }
	}

	public class VehicleWithLastPosition
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public bool IsActive { get; set; }
		public GpsPositionDto LastPosition { get; set; }
	}

	public class VehicleWithLastPositionResponse
	{
		public VehicleDto Vehicle { get; set; }
		public GpsPositionDto LastPosition { get; set; }
	}

	// GPS Position models
	public class GpsPositionDto
	{
		public long Id { get; set; }
		public int VehicleId { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }
		public DateTime RecordedAt { get; set; }
	}

	public class GpsPositionData
	{
		public double Latitude { get; set; }
		public double Longitude { get; set; }
		public DateTime RecordedAt { get; set; }
	}

	public class SubmitGpsPositionBatchRequest
	{
		public int VehicleId { get; set; }
		public List<GpsPositionData> Positions { get; set; }
	}

	// API Response wrapper
	public class ApiResponse<T>
	{
		public bool Success { get; set; }
		public T Data { get; set; }
		public string Message { get; set; }
		public List<string> Errors { get; set; }
	}
}