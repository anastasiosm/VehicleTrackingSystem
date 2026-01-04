using System.Web.Http;
using Unity;
using Unity.Lifetime;
using Unity.AspNet.WebApi;
using VehicleTracking.Core.Interfaces;
using VehicleTracking.Core.Services;
using VehicleTracking.Data.Context;
using VehicleTracking.Data.Repositories;

namespace VehicleTracking.Web.App_Start
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
			var container = new UnityContainer();

            // Register all your components with the container here
            // it is NOT necessary to register your controllers

            // Database Context
            container.RegisterType<VehicleTrackingContext>(new HierarchicalLifetimeManager());

            // Repositories
            container.RegisterType<IVehicleRepository, VehicleRepository>();
            container.RegisterType<IGpsPositionRepository, GpsPositionRepository>();

            // Services
            container.RegisterType<GpsService>();

            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }
    }
}