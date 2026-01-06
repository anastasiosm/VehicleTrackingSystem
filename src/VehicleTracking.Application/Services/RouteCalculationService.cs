using System;
using System.Collections.Generic;
using System.Linq;
using VehicleTracking.Application.Dtos;
using VehicleTracking.Application.Interfaces;
using VehicleTracking.Domain.Entities;
using VehicleTracking.Domain.ValueObjects;

namespace VehicleTracking.Application.Services
{
    /// <summary>
    /// Service responsible for calculating routes and distances from GPS positions.
    /// Follows Single Responsibility Principle - handles only route calculation logic.
    /// </summary>
    public class RouteCalculationService : IRouteCalculationService
    {
        private readonly IGeographicalService _geographicalService;

        public RouteCalculationService(IGeographicalService geographicalService)
        {
            _geographicalService = geographicalService ?? throw new ArgumentNullException(nameof(geographicalService));
        }

        /// <summary>
        /// Calculates the total distance traveled based on a sequence of GPS positions.
        /// Uses great-circle distance calculation between consecutive points.
        /// </summary>
        /// <param name="positions">Ordered list of GPS positions (chronological order)</param>
        /// <returns>Total distance in meters</returns>
        public double CalculateTotalDistance(IEnumerable<GpsPosition> positions)
        {
            if (positions == null)
                throw new ArgumentNullException(nameof(positions));

            var positionList = positions.ToList();
            if (positionList.Count < 2)
                return 0;

            double totalDistance = 0;
            for (int i = 0; i < positionList.Count - 1; i++)
            {
                var from = new Coordinates(positionList[i].Latitude, positionList[i].Longitude);
                var to = new Coordinates(positionList[i + 1].Latitude, positionList[i + 1].Longitude);
                
                totalDistance += _geographicalService.CalculateDistance(from, to);
            }

            return totalDistance;
        }

        /// <summary>
        /// Calculates route statistics including total distance, average speed, and time duration.
        /// </summary>
        /// <param name="positions">Ordered list of GPS positions</param>
        /// <returns>Route statistics with calculated metrics</returns>
        public RouteStatistics CalculateRouteStatistics(IEnumerable<GpsPosition> positions)
        {
            if (positions == null)
                throw new ArgumentNullException(nameof(positions));

            var positionList = positions.ToList();
            if (!positionList.Any())
            {
                return new RouteStatistics
                {
                    TotalDistanceMeters = 0,
                    PositionCount = 0,
                    DurationSeconds = 0,
                    AverageSpeedMetersPerSecond = 0
                };
            }

            var totalDistance = CalculateTotalDistance(positionList);
            var duration = (positionList.Last().RecordedAt - positionList.First().RecordedAt).TotalSeconds;

            return new RouteStatistics
            {
                TotalDistanceMeters = totalDistance,
                PositionCount = positionList.Count,
                DurationSeconds = duration,
                AverageSpeedMetersPerSecond = duration > 0 ? totalDistance / duration : 0
            };
        }
    }
}
