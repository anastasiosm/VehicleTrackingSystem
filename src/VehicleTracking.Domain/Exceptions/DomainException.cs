using System;

namespace VehicleTracking.Domain.Exceptions
{
    /// <summary>
    /// Base exception for domain rule violations.
    /// Domain exceptions represent violations of business rules and invariants,
    /// not infrastructure or application-level concerns.
    /// </summary>
    /// <remarks>
    /// Examples of domain exceptions:
    /// - Invalid coordinate ranges
    /// - Business rule violations (e.g., duplicate position timestamps)
    /// - Invariant violations within aggregates
    /// 
    /// NOT for:
    /// - Database errors (Infrastructure)
    /// - Entity not found (Application)
    /// - HTTP validation (Application)
    /// </remarks>
    public abstract class DomainException : Exception
    {
        protected DomainException(string message) : base(message)
        {
        }

        protected DomainException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}
