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

		// Default configuration values
		private const string DEFAULT_API_BASE_URL = "http://localhost:5000";
		private const int DEFAULT_INTERVAL_SECONDS = 30;
		private const int DEFAULT_POSITIONS_PER_VEHICLE = 5;
		private const double DEFAULT_RADIUS_METERS = 50.0;

		// Sleep and console clear settings
		private const int SLEEP_MILLISECONDS = 1000;
		private const int CONSOLE_CLEAR_SPACES = 50;

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
					Thread.Sleep(SLEEP_MILLISECONDS);
				}

				if (_running)
				{
					Console.Write("\r" + new string(' ', CONSOLE_CLEAR_SPACES) + "\r"); // Clear the line
				}
			}
		}

		private static GeneratorConfig LoadConfiguration()
		{
			return new GeneratorConfig
			{
				ApiBaseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"] ?? DEFAULT_API_BASE_URL,
				IntervalSeconds = int.Parse(ConfigurationManager.AppSettings["IntervalSeconds"] ?? DEFAULT_INTERVAL_SECONDS.ToString()),
				PositionsPerVehicle = int.Parse(ConfigurationManager.AppSettings["PositionsPerVehicle"] ?? DEFAULT_POSITIONS_PER_VEHICLE.ToString()),
				RadiusMeters = double.Parse(ConfigurationManager.AppSettings["RadiusMeters"] ?? DEFAULT_RADIUS_METERS.ToString())
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