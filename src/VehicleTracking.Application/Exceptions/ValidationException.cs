using System;

namespace VehicleTracking.Application.Exceptions
{
    /// <summary>
    /// Exception thrown when input validation fails.
    /// This is an APPLICATION concern for validating use case inputs,
    /// not a domain rule violation.
    /// </summary>
    public class ValidationException : ApplicationException
    {
        public ValidationException(string message) : base(message)
        {
        }

        public ValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
