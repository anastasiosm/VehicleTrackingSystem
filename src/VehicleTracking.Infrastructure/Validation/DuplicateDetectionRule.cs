using VehicleTracking.Application.Dtos;
using VehicleTracking.Application.Interfaces;
using VehicleTracking.Application.Validation;
using VehicleTracking.Domain.Entities;

namespace VehicleTracking.Infrastructure.Validation
{
    /// <summary>
    /// Validation rule that ensures no duplicate GPS positions exist for the same vehicle and timestamp.
    /// </summary>
    public class DuplicateDetectionRule : IValidationRule<GpsPosition>
    {
        private readonly IGpsPositionRepository _gpsPositionRepository;

        public DuplicateDetectionRule(IGpsPositionRepository gpsPositionRepository)
        {
            _gpsPositionRepository = gpsPositionRepository;
        }

        public ValidationResult Validate(GpsPosition position)
        {
            var exists = _gpsPositionRepository.PositionExists(position.VehicleId, position.RecordedAt);
            
            return exists
                ? ValidationResult.Failure($"Position for vehicle {position.VehicleId} at {position.RecordedAt} already exists.")
                : ValidationResult.Success();
        }
    }
}
