using System.Threading.Tasks;
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

        public async Task<ValidationResult> ValidateAsync(GpsPosition position)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(position.VehicleId);
            
            return vehicle == null 
                ? ValidationResult.Failure($"Vehicle with ID {position.VehicleId} does not exist.")
                : ValidationResult.Success();
        }
    }
}
