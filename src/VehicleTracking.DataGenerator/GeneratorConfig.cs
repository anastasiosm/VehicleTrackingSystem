namespace VehicleTracking.DataGenerator
{
	public class GeneratorConfig
	{
		public string ApiBaseUrl { get; set; }
		public int IntervalSeconds { get; set; }
		public int PositionsPerVehicle { get; set; }
		public double RadiusMeters { get; set; }
	}

	public class GenerationResult
	{
		public int VehiclesProcessed { get; set; }
		public int TotalPositionsSubmitted { get; set; }
		public int FailedSubmissions { get; set; }
	}
}
