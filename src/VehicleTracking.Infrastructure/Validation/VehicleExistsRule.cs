using VehicleTracking.Application.Dtos;
using VehicleTracking.Application.Interfaces;
using VehicleTracking.Application.Validation;
using VehicleTracking.Domain.Entities;

namespace VehicleTracking.Infrastructure.Validation
{
    /// <summary>
    /// Validation rule that ensures a vehicle exists in the system.
    /// </summary>
    public class VehicleExistsRule : IValidationRule<GpsPosition>
    {
        private readonly IVehicleRepository _vehicleRepository;

        public VehicleExistsRule(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }

        public ValidationResult Validate(GpsPosition position)
        {
            var vehicle = _vehicleRepository.GetById(position.VehicleId);
            
            return vehicle == null 
                ? ValidationResult.Failure($"Vehicle with ID {position.VehicleId} does not exist.")
                : ValidationResult.Success();
        }
    }
}
