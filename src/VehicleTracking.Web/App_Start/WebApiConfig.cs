using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace VehicleTracking.Web.App_Start
{
	public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
			// Enable attribute routing
			config.MapHttpAttributeRoutes();

			// Configure JSON serialization
			var jsonSettings = config.Formatters.JsonFormatter.SerializerSettings;
			jsonSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
			jsonSettings.Formatting = Formatting.Indented;
			jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
			jsonSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

			// Remove XML formatter - only JSON
			config.Formatters.Remove(config.Formatters.XmlFormatter);

			// Enable CORS for development
			// config.EnableCors();

			// Default route (fallback)
			config.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "api/{controller}/{id}",
				defaults: new { id = RouteParameter.Optional }
			);
		}
	}
}
