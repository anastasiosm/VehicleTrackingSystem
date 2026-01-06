using System;
using System.Collections.Generic;
using System.Linq;
using VehicleTracking.Domain.Entities;
using VehicleTracking.Application.Interfaces;
using VehicleTracking.Application.Dtos;
using VehicleTracking.Domain.Exceptions;
using AutoMapper;
using VehicleTracking.Domain.ValueObjects;

namespace VehicleTracking.Application.Services
{
    /// <summary>
    /// Implementation of GPS service providing business logic for position tracking.
    /// Orchestrates validation and persistence of GPS data.
    /// Delegates route calculation to IRouteCalculationService following SRP.
    /// </summary>
    public class GpsService : IGpsService
    {
        private readonly IGpsPositionRepository _gpsPositionRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IGpsPositionValidator _validator;
        private readonly IRouteCalculationService _routeCalculationService;
        private readonly IMapper _mapper;

        private const int DEFAULT_HOURS_BACK = 24; // Default time range for queries

        public GpsService(
            IGpsPositionRepository gpsPositionRepository,
            IVehicleRepository vehicleRepository,
            IGpsPositionValidator validator,
            IRouteCalculationService routeCalculationService,
            IMapper mapper)
        {
            _gpsPositionRepository = gpsPositionRepository ?? throw new ArgumentNullException(nameof(gpsPositionRepository));
            _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _routeCalculationService = routeCalculationService ?? throw new ArgumentNullException(nameof(routeCalculationService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public bool SubmitPosition(GpsPosition position)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));

            var validationResult = _validator.ValidatePosition(position);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(string.Join(" ", validationResult.Errors));
            }

            _gpsPositionRepository.Add(position);
            _gpsPositionRepository.SaveChanges();

            return true;
        }

        public bool SubmitPositions(IEnumerable<GpsPosition> positions)
        {
            if (positions == null)
                throw new ArgumentNullException(nameof(positions));

            var positionsList = positions.ToList();
            if (!positionsList.Any())
                return false;

            var positionValidationErrors = new List<string>();
            foreach (var position in positionsList)
            {
                var positionValidationResult = _validator.ValidatePosition(position);
                if (!positionValidationResult.IsValid)
                {
                    positionValidationErrors.AddRange(positionValidationResult.Errors);
                }
            }

            if (positionValidationErrors.Any())
            {
                throw new ValidationException(string.Join(" ", positionValidationErrors));
            }

            _gpsPositionRepository.AddRange(positionsList);
            _gpsPositionRepository.SaveChanges();

            return true;
        }

        public RouteResultDto GetRoute(int vehicleId, DateTime from, DateTime to)
        {
            var vehicle = _vehicleRepository.GetById(vehicleId);
            if (vehicle == null)
                throw new EntityNotFoundException("Vehicle", vehicleId);

            var positions = _gpsPositionRepository.GetPositionsForVehicle(vehicleId, from, to).ToList();
            var positionDtos = _mapper.Map<List<GpsPositionDto>>(positions);

            if (positionDtos == null)
                throw new InvalidOperationException("Failed to map GPS positions to DTOs");

            // Delegate distance calculation to RouteCalculationService (SRP)
            var totalDistance = _routeCalculationService.CalculateTotalDistance(positions);

            return new RouteResultDto
            {
                VehicleId = vehicleId,
                VehicleName = vehicle.Name,
                Positions = positionDtos,
                TotalDistanceMeters = totalDistance,
                PositionCount = positions.Count
            };
        }

        public GpsPositionDto GetLastPosition(int vehicleId)
        {
            var vehicle = _vehicleRepository.GetById(vehicleId);
            if (vehicle == null)
                throw new EntityNotFoundException("Vehicle", vehicleId);

            var position = _gpsPositionRepository.GetLastPositionForVehicle(vehicleId);
            
            // Position can be null if vehicle has no GPS data yet
            if (position == null)
                return null;

            var positionDto = _mapper.Map<GpsPositionDto>(position);
            if (positionDto == null)
                throw new InvalidOperationException($"Failed to map GPS position to DTO for vehicle {vehicleId}");

            return positionDto;
        }
    }
}
