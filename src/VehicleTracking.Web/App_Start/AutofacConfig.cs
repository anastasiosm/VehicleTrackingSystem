using VehicleTracking.Domain.ValueObjects;
using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using AutoMapper;
using Serilog;
using VehicleTracking.Application.Interfaces;
using VehicleTracking.Application.Services;
using VehicleTracking.Persistence.Context;
using VehicleTracking.Persistence.Repositories;
using VehicleTracking.Infrastructure.Services;
using VehicleTracking.Infrastructure.Validation;
using VehicleTracking.Application.Validation;
using VehicleTracking.Domain.Entities;
using VehicleTracking.Web.Services;

namespace VehicleTracking.Web.App_Start
{
	public static class AutofacConfig
	{
		public static void Register()
		{
			var builder = new ContainerBuilder();

			var config = GlobalConfiguration.Configuration;

			// Register Serilog
			builder.RegisterInstance(Log.Logger).As<ILogger>().SingleInstance();

			builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

			builder.RegisterWebApiFilterProvider(config);

			// Database Context - one per HTTP request
			builder.RegisterType<VehicleTrackingContext>().AsSelf().InstancePerRequest();

			// Repositories
			builder.RegisterType<VehicleRepository>().As<IVehicleRepository>().InstancePerRequest();
			builder.RegisterType<GpsPositionRepository>().As<IGpsPositionRepository>().InstancePerRequest();

			// Core Services
			builder.RegisterType<VehicleService>().As<IVehicleService>().InstancePerRequest();
			builder.RegisterType<GpsService>().As<IGpsService>().InstancePerRequest();
			builder.RegisterType<RouteCalculationService>().As<IRouteCalculationService>().InstancePerRequest();

			// GPS Validation - Strategy Pattern with Composite Validator
			builder.RegisterType<VehicleExistsRule>().As<IValidationRule<GpsPosition>>().InstancePerRequest();
			builder.RegisterType<VehicleActiveRule>().As<IValidationRule<GpsPosition>>().InstancePerRequest();
			builder.RegisterType<GeographicBoundsRule>().As<IValidationRule<GpsPosition>>().InstancePerRequest();
			builder.RegisterType<DuplicateDetectionRule>().As<IValidationRule<GpsPosition>>().InstancePerRequest();
			builder.RegisterType<CompositeGpsPositionValidator>().As<IGpsPositionValidator>().InstancePerRequest();

			// Geographical Services
			builder.RegisterType<AthensBoundingBoxProvider>().As<IBoundingBoxProvider>().InstancePerRequest();
			builder.RegisterType<GeographicalService>().As<IGeographicalService>().InstancePerRequest();

			// Web Layer Services - Mapping & Response Building
			builder.RegisterType<VehiclePositionMapper>().As<IVehiclePositionMapper>().InstancePerRequest();
			builder.RegisterType<ApiResponseBuilder>().As<IApiResponseBuilder>().InstancePerRequest();

			// AutoMapper - singleton instance
			var mapperConfig = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile<MappingProfile>();
			});
			builder.RegisterInstance(mapperConfig.CreateMapper()).As<IMapper>().SingleInstance();

			var container = builder.Build();
			config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
		}
	}
}

