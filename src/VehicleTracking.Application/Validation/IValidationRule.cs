using VehicleTracking.Application.Dtos;

namespace VehicleTracking.Application.Validation
{
    /// <summary>
    /// Represents a validation rule that can be applied to an entity.
    /// Follows Strategy Pattern to allow flexible composition of validation logic.
    /// </summary>
    /// <typeparam name="T">The type of entity to validate</typeparam>
    public interface IValidationRule<T>
    {
        /// <summary>
        /// Validates the specified entity against this rule.
        /// </summary>
        /// <param name="entity">The entity to validate</param>
        /// <returns>Validation result indicating success or failure with error messages</returns>
        ValidationResult Validate(T entity);
    }
}
