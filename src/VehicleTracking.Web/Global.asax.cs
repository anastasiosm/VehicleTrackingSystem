using System.Data.Entity;
using System.Web.Http;
using VehicleTracking.Data.Context;
using VehicleTracking.Data.Initializers;
using VehicleTracking.Web.App_Start;

namespace VehicleTracking.Web
{
	public class WebApiApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
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
	}
}
