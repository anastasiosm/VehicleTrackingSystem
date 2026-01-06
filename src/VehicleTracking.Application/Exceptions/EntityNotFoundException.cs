using System;

namespace VehicleTracking.Application.Exceptions
{
    /// <summary>
    /// Exception thrown when a requested entity is not found in the system.
    /// This is an APPLICATION concern, not a domain rule violation.
    /// Use this for "404 Not Found" scenarios where a resource doesn't exist.
    /// </summary>
    public class EntityNotFoundException : ApplicationException
    {
        public string EntityName { get; }
        public object EntityId { get; }

        public EntityNotFoundException(string entityName, object entityId)
            : base($"{entityName} with ID '{entityId}' was not found.")
        {
            EntityName = entityName;
            EntityId = entityId;
        }

        public EntityNotFoundException(string entityName, object entityId, string message)
            : base(message)
        {
            EntityName = entityName;
            EntityId = entityId;
        }

        public EntityNotFoundException(string message) : base(message)
        {
        }

        public EntityNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
