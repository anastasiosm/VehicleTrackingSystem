using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VehicleTracking.Application.Dtos;
using VehicleTracking.Application.Interfaces;
using VehicleTracking.Application.Validation;
using VehicleTracking.Domain.Entities;

namespace VehicleTracking.Infrastructure.Validation
{
    /// <summary>
    /// Composite GPS position validator that applies multiple validation rules.
    /// Implements Strategy Pattern and Composite Pattern for flexible validation logic.
    /// </summary>
    public class CompositeGpsPositionValidator : IGpsPositionValidator
    {
        private readonly IEnumerable<IValidationRule<GpsPosition>> _rules;

        public CompositeGpsPositionValidator(IEnumerable<IValidationRule<GpsPosition>> rules)
        {
            _rules = rules;
        }

        public async Task<ValidationResult> ValidatePositionAsync(GpsPosition position)
        {
            var errors = new List<string>();

            foreach (var rule in _rules)
            {
                var result = await rule.ValidateAsync(position);
                if (!result.IsValid)
                {
                    errors.AddRange(result.Errors);
                }
            }

            return errors.Any()
                ? ValidationResult.Failure(errors.ToArray())
                : ValidationResult.Success();
        }

        public Task<ValidationResult> ValidateBatchAsync(IEnumerable<GpsPosition> positions)
        {
            if (positions == null || !positions.Any())
            {
                return Task.FromResult(ValidationResult.Failure("Positions collection cannot be null or empty."));
            }

            var vehicleId = positions.First().VehicleId;

            if (positions.Any(p => p.VehicleId != vehicleId))
            {
                return Task.FromResult(ValidationResult.Failure("All positions must belong to the same vehicle."));
            }

            // For batch validation, we only check common constraints
            // Individual position validation happens during processing
            return Task.FromResult(ValidationResult.Success());
        }
    }
}
