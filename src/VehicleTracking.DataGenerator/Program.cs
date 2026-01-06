using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Serilog;
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
			// Initialize Serilog
			var logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "generator.log");
			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Debug()
				.WriteTo.Console()
				.WriteTo.File(logPath)
				.CreateLogger();

			Log.Information("==============================================");
			Log.Information("Vehicle Tracking - GPS Data Generator");
			Log.Information("==============================================");

			try 
			{
				// Read configuration
				var config = LoadConfiguration();
				DisplayConfiguration(config);

				// Setup DI and resolve generator
				var container = AutofacConfig.Configure(config);
				var generator = container.Resolve<GpsDataGenerator>();

				Log.Information("Press CTRL+C to stop the generator...");
				Console.WriteLine();

				// Handle CTRL+C gracefully
							Console.CancelKeyPress += (sender, e) =>
							{
								e.Cancel = true;
								_running = false;
								Log.Warning("Shutting down gracefully...");
							};
				// Run the generator loop
				RunGeneratorLoop(generator, config).Wait();
			}
			catch (Exception ex)
			{
				Log.Fatal(ex, "Application terminated unexpectedly");
			}
			finally
			{
				Log.CloseAndFlush();
			}

			Log.Information("Generator stopped. Press any key to exit...");
			Console.ReadKey();
		}

		private static async Task RunGeneratorLoop(GpsDataGenerator generator, GeneratorConfig config)
		{
			int iteration = 0;

			while (_running)
			{
				iteration++;
				Log.Information("Starting iteration #{Iteration}...", iteration);

				try
				{
					var result = await generator.GenerateAndSubmitPositionsAsync();

					Log.Information("Iteration #{Iteration} completed. Vehicles: {Processed}, Positions: {Total}, Failed: {Failed}", 
						iteration, result.VehiclesProcessed, result.TotalPositionsSubmitted, result.FailedSubmissions);
				}
				catch (Exception ex)
				{
					Log.Error(ex, "Error in iteration #{Iteration}", iteration);
				}

				// Wait for N seconds before next iteration
				for (int i = config.IntervalSeconds; i > 0 && _running; i--)
				{
					Console.Write($"\rNext iteration in {i} seconds...  ");
					Thread.Sleep(1000);
				}

				if (_running)
				{
					Console.Write("\r" + new string(' ', 50) + "\r"); // Clear the line
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
			Log.Information("Configuration:");
			Log.Information("  API Base URL: {ApiBaseUrl}", config.ApiBaseUrl);
			Log.Information("  Interval: {IntervalSeconds} seconds", config.IntervalSeconds);
			Log.Information("  Positions per vehicle: {PositionsPerVehicle}", config.PositionsPerVehicle);
			Log.Information("  Movement radius: {RadiusMeters} meters", config.RadiusMeters);
		}
	}
}