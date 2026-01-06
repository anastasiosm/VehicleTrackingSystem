using VehicleTracking.Domain.ValueObjects;
using System.Collections.Generic;
using System.Linq;
using VehicleTracking.Domain.Entities;
using VehicleTracking.Application.Interfaces;
using VehicleTracking.Application.Dtos;


namespace VehicleTracking.Infrastructure.Services
{
    /// <summary>
    /// Provides validation logic for GPS position data, ensuring that positions are associated with active vehicles,
    /// are within allowed geographical boundaries, and do not duplicate existing entries.
    /// </summary>
    /// <remarks>This class is typically used to validate individual GPS positions or batches of positions
    /// before they are processed or stored. It relies on external repositories and services to verify vehicle status,
    /// geographical constraints, and position uniqueness. Thread safety depends on the underlying repository and
    /// service implementations.</remarks>
    public class GpsPositionValidator : IGpsPositionValidator
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IGpsPositionRepository _gpsPositionRepository;
        private readonly IGeographicalService _geographicalService;
        private readonly IBoundingBoxProvider _boundingBoxProvider;

        public GpsPositionValidator(
            IVehicleRepository vehicleRepository,
            IGpsPositionRepository gpsPositionRepository,
            IGeographicalService geographicalService,
            IBoundingBoxProvider boundingBoxProvider)
        {
            _vehicleRepository = vehicleRepository;
            _gpsPositionRepository = gpsPositionRepository;
            _geographicalService = geographicalService;
            _boundingBoxProvider = boundingBoxProvider;
        }

        /// <summary>
        /// Validates a GPS position for a vehicle, checking for vehicle existence, activity status, boundary
        /// constraints, and duplicate entries.
        /// </summary>
        /// <remarks>Validation includes verifying that the vehicle exists and is active, that the
        /// coordinates are within the allowed geographical boundary, and that the position does not already exist for
        /// the specified vehicle and timestamp.</remarks>
        /// <param name="position">The GPS position to validate. Must contain a valid vehicle identifier, latitude, longitude, and timestamp.</param>
        /// <returns>A ValidationResult indicating whether the position is valid. If validation fails, the result contains one or
        /// more error messages describing the issues.</returns>
        public ValidationResult ValidatePosition(GpsPosition position)
        {
            var errors = new List<string>();

            var vehicle = _vehicleRepository.GetById(position.VehicleId);
            if (vehicle == null)
            {
                errors.Add($"Vehicle with ID {position.VehicleId} does not exist.");
            }
            else
            {
                if (!vehicle.IsActive)
                {
                    errors.Add($"Vehicle {vehicle.Name} is inactive and cannot accept new positions.");
                }
            }

            var boundingBox = _boundingBoxProvider.GetBoundingBox();
            if (!_geographicalService.IsWithinBoundary(position.Latitude, position.Longitude, boundingBox))
            {
                errors.Add($"Coordinates ({position.Latitude}, {position.Longitude}) are outside the allowed area.");
            }

            if (_gpsPositionRepository.PositionExists(position.VehicleId, position.RecordedAt))
            {
                errors.Add($"Position for vehicle {position.VehicleId} at {position.RecordedAt} already exists.");
            }

            return errors.Any()
                ? ValidationResult.Failure(errors.ToArray())
                : ValidationResult.Success();
        }

        /// <summary>
        /// Validates a batch of GPS positions to ensure they are suitable for processing.
        /// </summary>
        /// <param name="positions">A collection of GPS positions to validate. All positions must belong to the same active vehicle. Cannot be
        /// null or empty.</param>
        /// <returns>A ValidationResult indicating whether the batch is valid. Returns a failure result if the collection is null
        /// or empty, if positions belong to different vehicles, if the vehicle does not exist, or if the vehicle is
        /// inactive; otherwise, returns a success result.</returns>
        public ValidationResult ValidateBatch(IEnumerable<GpsPosition> positions)
        {
            if (positions == null || !positions.Any())
            {
                return ValidationResult.Failure("Positions collection cannot be null or empty.");
            }

            var vehicleId = positions.First().VehicleId;

            if (positions.Any(p => p.VehicleId != vehicleId))
            {
                return ValidationResult.Failure("All positions must belong to the same vehicle.");
            }

            var vehicle = _vehicleRepository.GetById(vehicleId);
            if (vehicle == null)
            {
                return ValidationResult.Failure($"Vehicle with ID {vehicleId} does not exist.");
            }

            if (!vehicle.IsActive)
            {
                return ValidationResult.Failure($"Vehicle {vehicle.Name} is inactive and cannot accept new positions.");
            }

            return ValidationResult.Success();
        }
    }
}

