using System;
using System.Runtime.Caching;
using VehicleTracking.Application.Dtos;
using VehicleTracking.Application.Interfaces;

namespace VehicleTracking.Infrastructure.Caching
{
    /// <summary>
    /// Caching decorator for IGpsService implementing cache-aside pattern.
    /// Caches frequently accessed data like last vehicle positions.
    /// </summary>
    public class CachedGpsService : IGpsService
    {
        private readonly IGpsService _innerService;
        private readonly MemoryCache _cache;
        
        private const int CACHE_DURATION_SECONDS = 30; // Cache for 30 seconds
        private const string LAST_POSITION_KEY_PREFIX = "LastPosition_";

        public CachedGpsService(IGpsService innerService)
        {
            _innerService = innerService ?? throw new ArgumentNullException(nameof(innerService));
            _cache = MemoryCache.Default;
        }

        public bool SubmitPosition(Domain.Entities.GpsPosition position)
        {
            // Invalidate cache on write
            var cacheKey = GetLastPositionCacheKey(position.VehicleId);
            _cache.Remove(cacheKey);

            return _innerService.SubmitPosition(position);
        }

        public bool SubmitPositions(System.Collections.Generic.IEnumerable<Domain.Entities.GpsPosition> positions)
        {
            // Invalidate cache for all affected vehicles
            foreach (var position in positions)
            {
                var cacheKey = GetLastPositionCacheKey(position.VehicleId);
                _cache.Remove(cacheKey);
            }

            return _innerService.SubmitPositions(positions);
        }

        public RouteResultDto GetRoute(int vehicleId, DateTime from, DateTime to)
        {
            // Don't cache routes - they vary by time range
            return _innerService.GetRoute(vehicleId, from, to);
        }

        public GpsPositionDto GetLastPosition(int vehicleId)
        {
            var cacheKey = GetLastPositionCacheKey(vehicleId);

            // Try to get from cache
            if (_cache.Get(cacheKey) is GpsPositionDto cachedPosition)
            {
                return cachedPosition;
            }

            // Cache miss - get from service
            var position = _innerService.GetLastPosition(vehicleId);

            // Store in cache (only if position exists)
            if (position != null)
            {
                var policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.UtcNow.AddSeconds(CACHE_DURATION_SECONDS)
                };
                _cache.Set(cacheKey, position, policy);
            }

            return position;
        }

        private static string GetLastPositionCacheKey(int vehicleId)
        {
            return $"{LAST_POSITION_KEY_PREFIX}{vehicleId}";
        }
    }
}
