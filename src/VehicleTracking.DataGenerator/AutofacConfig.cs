using Autofac;
using Serilog;
using VehicleTracking.Application.Interfaces;
using VehicleTracking.DataGenerator.Services;
using VehicleTracking.Infrastructure.Services;

namespace VehicleTracking.DataGenerator
{
    public static class AutofacConfig
    {
        public static IContainer Configure(GeneratorConfig config)
        {
            var builder = new ContainerBuilder();

            // Register Serilog ILogger
            builder.RegisterInstance(Log.Logger).As<ILogger>().SingleInstance();

            // Register Configuration
            builder.RegisterInstance(config).As<GeneratorConfig>();

            // Register Services
            builder.RegisterType<AthensBoundingBoxProvider>().As<IBoundingBoxProvider>();
            builder.RegisterType<AthensPositionSimulator>().As<IPositionSimulator>();
            
            // Register VehicleApiClient with the Base URL from config
            builder.Register(c => new VehicleApiClient(config.ApiBaseUrl, c.Resolve<ILogger>()))
                   .As<IVehicleApiClient>();

            // Register the Generator itself
            builder.RegisterType<GpsDataGenerator>().AsSelf();

            return builder.Build();
        }
    }
}
