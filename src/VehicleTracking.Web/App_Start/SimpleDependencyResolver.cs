using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using AutoMapper;
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
        private readonly IMapper _mapper;
        private readonly bool _isScope;

        public SimpleDependencyResolver(IMapper mapper = null)
        {
            if (mapper == null)
            {
                // Root resolver initialization
                var config = new MapperConfiguration(cfg => {
                    cfg.AddProfile<MappingProfile>();
                });
                _mapper = config.CreateMapper();
                _isScope = false;
            }
            else
            {
                // Request scope initialization
                _mapper = mapper;
                _context = new VehicleTrackingContext();
                _vehicleRepository = new VehicleRepository(_context);
                _gpsPositionRepository = new GpsPositionRepository(_context);
                _gpsService = new GpsService(_gpsPositionRepository, _vehicleRepository);
                _isScope = true;
            }
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(VehiclesController))
            {
                return new VehiclesController(_vehicleRepository, _gpsPositionRepository, _mapper);
            }
            if (serviceType == typeof(GpsController))
            {
                return new GpsController(_gpsService, _vehicleRepository, _gpsPositionRepository, _mapper);
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
            // Return a new resolver instance that will act as the scope for a single request
            return new SimpleDependencyResolver(_mapper);
        }

        public void Dispose()
        {
            // Only dispose the context if this instance is a request scope
            if (_isScope)
            {
                _context?.Dispose();
            }
        }
    }
}