using VehicleTracking.Domain.Exceptions;
using System;

namespace VehicleTracking.Domain.Exceptions
{
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message)
        {
        }
    }
}

