using VehicleTracking.Application.Dtos;
using VehicleTracking.Application.Interfaces;
using VehicleTracking.Application.Validation;
using VehicleTracking.Domain.Entities;

namespace VehicleTracking.Infrastructure.Validation
{
    /// <summary>
    /// Validation rule that ensures a vehicle is active before accepting GPS positions.
    /// </summary>
    public class VehicleActiveRule : IValidationRule<GpsPosition>
    {
        private readonly IVehicleRepository _vehicleRepository;

        public VehicleActiveRule(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }

        public ValidationResult Validate(GpsPosition position)
        {
            var vehicle = _vehicleRepository.GetById(position.VehicleId);
            
            if (vehicle == null)
                return ValidationResult.Success(); // VehicleExistsRule will handle this
            
            return !vehicle.IsActive
                ? ValidationResult.Failure($"Vehicle {vehicle.Name} is inactive and cannot accept new positions.")
                : ValidationResult.Success();
        }
    }
}
