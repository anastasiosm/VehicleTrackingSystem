using System;
using System.Collections.Generic;
using System.Linq;
using VehicleTracking.Domain.Entities;
using VehicleTracking.Application.Interfaces;
using VehicleTracking.Application.Models;
using VehicleTracking.Domain.Exceptions;

namespace VehicleTracking.Application.Services
{
    /// <summary>
    /// Implementation of GPS service providing business logic for position tracking.
    /// Orchestrates validation and persistence of GPS data.
    /// </summary>
    public class GpsService : IGpsService
    {
        private readonly IGpsPositionRepository _gpsPositionRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IGpsPositionValidator _validator;

        public GpsService(
            IGpsPositionRepository gpsPositionRepository,
            IVehicleRepository vehicleRepository,
            IGpsPositionValidator validator)
        {
            _gpsPositionRepository = gpsPositionRepository;
            _vehicleRepository = vehicleRepository;
            _validator = validator;
        }

        public bool SubmitPosition(GpsPosition position)
        {
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
            if (positions == null || !positions.Any()) return false;

            var validationResult = _validator.ValidateBatch(positions);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(string.Join(" ", validationResult.Errors));
            }

            var validPositions = new List<GpsPosition>();
            foreach (var position in positions)
            {
                var itemResult = _validator.ValidatePosition(position);
                if (itemResult.IsValid)
                {
                    validPositions.Add(position);
                }
            }

            if (validPositions.Any())
            {
                _gpsPositionRepository.AddRange(validPositions);
                _gpsPositionRepository.SaveChanges();
            }

            return true;
        }

        public IEnumerable<GpsPosition> GetVehiclePositions(int vehicleId, DateTime startTime, DateTime endTime)
        {
            if (_vehicleRepository.GetById(vehicleId) == null)
            {
                throw new ValidationException($"Vehicle with ID {vehicleId} does not exist.");
            }

            return _gpsPositionRepository.GetPositionsForVehicle(vehicleId, startTime, endTime);
        }

        public GpsPosition GetLastPosition(int vehicleId)
        {
            if (_vehicleRepository.GetById(vehicleId) == null)
            {
                throw new ValidationException($"Vehicle with ID {vehicleId} does not exist.");
            }

            return _gpsPositionRepository.GetLastPositionForVehicle(vehicleId);
        }
    }
}
