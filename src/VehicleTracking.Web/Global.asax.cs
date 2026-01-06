using System.Data.Entity;
using System.Web.Http;
using VehicleTracking.Persistence.Context;
using VehicleTracking.Persistence.Initializers;
using VehicleTracking.Web.App_Start;
using Serilog;
using System.Web;
using System.IO;

namespace VehicleTracking.Web
{
	public class WebApiApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
			// Configure Serilog
			var logPath = Path.Combine(HttpContext.Current.Server.MapPath("~/App_Data"), "logs", "web.log");
			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Debug()
				.WriteTo.File(logPath)
				.CreateLogger();

			Log.Information("Vehicle Tracking Web API Starting...");

			// Configure Web API
			GlobalConfiguration.Configure(WebApiConfig.Register);

			// Configure Dependency Injection (Autofac)
			AutofacConfig.Register();

			// Initialize database
			Database.SetInitializer(new VehicleTrackingInitializer());

			// Force database initialization
			using (var context = new VehicleTrackingContext())
			{
				context.Database.Initialize(force: false);
			}
		}

		protected void Application_End()
		{
			Log.Information("Vehicle Tracking Web API Shutting down...");
			Log.CloseAndFlush();
		}
	}
}
