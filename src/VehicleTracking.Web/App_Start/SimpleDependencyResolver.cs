using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using VehicleTracking.Core.Interfaces;
using VehicleTracking.Core.Services;
using VehicleTracking.Data.Context;
using VehicleTracking.Data.Repositories;
using VehicleTracking.Web.Controllers;

namespace VehicleTracking.Web.App_Start
{
    public class SimpleDependencyResolver : IDependencyResolver
    {
        private readonly VehicleTrackingContext _context;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IGpsPositionRepository _gpsPositionRepository;
        private readonly GpsService _gpsService;

        public SimpleDependencyResolver()
        {
            // Χειροκίνητο instantiation των dependencies (Singleton-like για το request)
            _context = new VehicleTrackingContext();
            _vehicleRepository = new VehicleRepository(_context);
            _gpsPositionRepository = new GpsPositionRepository(_context);
            _gpsService = new GpsService(_gpsPositionRepository, _vehicleRepository);
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(VehiclesController))
            {
                return new VehiclesController(_vehicleRepository, _gpsPositionRepository);
            }
            if (serviceType == typeof(GpsController))
            {
                return new GpsController(_gpsService, _vehicleRepository, _gpsPositionRepository);
            }
            if (serviceType == typeof(ValuesController))
            {
                return new ValuesController();
            }
            return null;
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return new List<object>();
        }

        public IDependencyScope BeginScope()
        {
            return this;
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}