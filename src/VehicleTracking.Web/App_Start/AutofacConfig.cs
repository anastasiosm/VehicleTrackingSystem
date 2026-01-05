using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using AutoMapper;
using VehicleTracking.Core.Interfaces;
using VehicleTracking.Core.Services;
using VehicleTracking.Data.Context;
using VehicleTracking.Data.Repositories;

namespace VehicleTracking.Web.App_Start
{
    public static class AutofacConfig
    {
        public static void Register()
        {
            var builder = new ContainerBuilder();

            // Get your HttpConfiguration
            var config = GlobalConfiguration.Configuration;

            // Register your Web API controllers
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            // OPTIONAL: Register the Autofac filter provider
            builder.RegisterWebApiFilterProvider(config);

            // Register Database Context
            // Using InstancePerRequest to ensure one context per HTTP request
            builder.RegisterType<VehicleTrackingContext>().AsSelf().InstancePerRequest();

            // Register Repositories
            builder.RegisterType<VehicleRepository>().As<IVehicleRepository>().InstancePerRequest();
            builder.RegisterType<GpsPositionRepository>().As<IGpsPositionRepository>().InstancePerRequest();

            // Register Services
            builder.RegisterType<VehicleService>().As<IVehicleService>().InstancePerRequest();
            builder.RegisterType<GpsService>().As<IGpsService>().InstancePerRequest();

            // Register AutoMapper
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });
            builder.RegisterInstance(mapperConfig.CreateMapper()).As<IMapper>().SingleInstance();

            // Set the dependency resolver to be Autofac
            var container = builder.Build();
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
        }
    }
}