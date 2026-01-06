using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using VehicleTracking.DataGenerator.Services;
using VehicleTracking.Infrastructure.Services;
using VehicleTracking.Application.Interfaces;

namespace VehicleTracking.DataGenerator
{
	class Program
	{
		private static bool _running = true;

		static void Main(string[] args)
		{
			Console.WriteLine("==============================================");
			Console.WriteLine("Vehicle Tracking - GPS Data Generator");
			Console.WriteLine("==============================================");
			Console.WriteLine();

			// Read configuration
			var config = LoadConfiguration();
			DisplayConfiguration(config);

			// Setup DI and resolve generator
			var container = AutofacConfig.Configure(config);
			var generator = container.Resolve<GpsDataGenerator>();

			Console.WriteLine("Press CTRL+C to stop the generator...");
			Console.WriteLine();

			// Handle CTRL+C gracefully
			Console.CancelKeyPress += (sender, e) =>
			{
				e.Cancel = true;
				_running = false;
				Console.WriteLine("\nShutting down gracefully...");
			};

			// Run the generator loop
			RunGeneratorLoop(generator, config).Wait();

			Console.WriteLine("Generator stopped. Press any key to exit...");
			Console.ReadKey();
		}

		private static async Task RunGeneratorLoop(GpsDataGenerator generator, GeneratorConfig config)
		{
			int iteration = 0;

			while (_running)
			{
				iteration++;
				Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Starting iteration #{iteration}...");

				try
				{
					var result = await generator.GenerateAndSubmitPositionsAsync();

					Console.ForegroundColor = ConsoleColor.Green;
					Console.WriteLine($"  Done: Generated positions for {result.VehiclesProcessed} vehicles");
					Console.WriteLine($"  Done: Total positions submitted: {result.TotalPositionsSubmitted}");
					Console.WriteLine($"  Done: Failed submissions: {result.FailedSubmissions}");
					Console.ResetColor();
				}
				catch (Exception ex)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"  Error: {ex.Message}");
					Console.ResetColor();
				}

				Console.WriteLine();

				// Wait for N seconds before next iteration
				for (int i = config.IntervalSeconds; i > 0 && _running; i--)
				{
					Console.Write($"\rNext iteration in {i} seconds...  ");
					Thread.Sleep(1000);
				}

				if (_running)
				{
					Console.WriteLine("\r" + new string(' ', 50)); // Clear the line
				}
			}
		}

		private static GeneratorConfig LoadConfiguration()
		{
			return new GeneratorConfig
			{
				ApiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"] ?? "http://localhost:5000",
				IntervalSeconds = int.Parse(ConfigurationManager.AppSettings["IntervalSeconds"] ?? "30"),
				PositionsPerVehicle = int.Parse(ConfigurationManager.AppSettings["PositionsPerVehicle"] ?? "5"),
				RadiusMeters = double.Parse(ConfigurationManager.AppSettings["RadiusMeters"] ?? "50")
			};
		}

		private static void DisplayConfiguration(GeneratorConfig config)
		{
			Console.WriteLine("Configuration:");
			Console.WriteLine($"  API Base URL: {config.ApiBaseUrl}");
			Console.WriteLine($"  Interval: {config.IntervalSeconds} seconds");
			Console.WriteLine($"  Positions per vehicle: {config.PositionsPerVehicle}");
			Console.WriteLine($"  Movement radius: {config.RadiusMeters} meters");
			Console.WriteLine();
		}
	}
}