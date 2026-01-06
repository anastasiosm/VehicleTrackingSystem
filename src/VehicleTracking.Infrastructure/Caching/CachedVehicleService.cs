using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using VehicleTracking.Application.Dtos;
using VehicleTracking.Application.Interfaces;

namespace VehicleTracking.Infrastructure.Caching
{
    /// <summary>
    /// Caching decorator for IVehicleService using cache-aside pattern.
    /// Caches vehicle lists which change infrequently.
    /// </summary>
    public class CachedVehicleService : IVehicleService
    {
        private readonly IVehicleService _innerService;
        private readonly MemoryCache _cache;
        
        private const int CACHE_DURATION_MINUTES = 5;
        private const string ALL_VEHICLES_KEY = "AllVehicles";
        private const string VEHICLE_WITH_POSITIONS_KEY = "VehiclesWithPositions";
        private const string VEHICLE_KEY_PREFIX = "Vehicle_";

        public CachedVehicleService(IVehicleService innerService)
        {
            _innerService = innerService ?? throw new ArgumentNullException(nameof(innerService));
            _cache = MemoryCache.Default;
        }

        public IEnumerable<VehicleDto> GetAllVehicles()
        {
            // Try cache first
            if (_cache.Get(ALL_VEHICLES_KEY) is IEnumerable<VehicleDto> cachedVehicles)
            {
                return cachedVehicles;
            }

            // Cache miss - fetch from service
            var vehicles = _innerService.GetAllVehicles();

            // Store in cache
            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(CACHE_DURATION_MINUTES)
            };
            _cache.Set(ALL_VEHICLES_KEY, vehicles, policy);

            return vehicles;
        }

        public VehicleDto GetVehicleById(int id)
        {
            var cacheKey = $"{VEHICLE_KEY_PREFIX}{id}";

            // Try cache
            if (_cache.Get(cacheKey) is VehicleDto cachedVehicle)
            {
                return cachedVehicle;
            }

            // Cache miss
            var vehicle = _innerService.GetVehicleById(id);

            // Store in cache
            if (vehicle != null)
            {
                var policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(CACHE_DURATION_MINUTES)
                };
                _cache.Set(cacheKey, vehicle, policy);
            }

            return vehicle;
        }

        public IEnumerable<VehicleWithPositionDto> GetVehiclesWithLastPositions()
        {
            // Try cache
            if (_cache.Get(VEHICLE_WITH_POSITIONS_KEY) is IEnumerable<VehicleWithPositionDto> cachedData)
            {
                return cachedData;
            }

            // Cache miss
            var data = _innerService.GetVehiclesWithLastPositions();

            // Store in cache (shorter duration because positions change frequently)
            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(30) // 30 seconds for position data
            };
            _cache.Set(VEHICLE_WITH_POSITIONS_KEY, data, policy);

            return data;
        }

        public VehicleWithPositionDto GetVehicleWithLastPosition(int id)
        {
            // Don't cache individual vehicle positions - they change too frequently
            // Or use very short cache duration
            return _innerService.GetVehicleWithLastPosition(id);
        }
    }
}
