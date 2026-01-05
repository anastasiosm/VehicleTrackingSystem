using System.Collections.Generic;
using System.Linq;

namespace VehicleTracking.Application.Models
{
	/// <summary>
	/// Represents the result of a validation operation, including success status and error messages. 
	/// </summary>
	public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        public static ValidationResult Success()
        {
            return new ValidationResult { IsValid = true };
        }

        public static ValidationResult Failure(params string[] errors)
        {
            return new ValidationResult
            {
                IsValid = false,
                Errors = errors.ToList()
            };
        }
    }
}
