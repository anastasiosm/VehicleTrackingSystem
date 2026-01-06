using System;

namespace VehicleTracking.Application.Exceptions
{
    /// <summary>
    /// Base exception for application-level errors.
    /// Application exceptions represent use case failures, validation errors,
    /// and orchestration problems, not domain rule violations.
    /// </summary>
    public abstract class ApplicationException : Exception
    {
        protected ApplicationException(string message) : base(message)
        {
        }

        protected ApplicationException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}
