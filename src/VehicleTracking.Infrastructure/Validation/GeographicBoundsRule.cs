using VehicleTracking.Application.Dtos;
using VehicleTracking.Application.Interfaces;
using VehicleTracking.Application.Validation;
using VehicleTracking.Domain.Entities;

namespace VehicleTracking.Infrastructure.Validation
{
    /// <summary>
    /// Validation rule that ensures GPS coordinates are within the allowed geographical boundary.
    /// </summary>
    public class GeographicBoundsRule : IValidationRule<GpsPosition>
    {
        private readonly IGeographicalService _geographicalService;
        private readonly IBoundingBoxProvider _boundingBoxProvider;

        public GeographicBoundsRule(
            IGeographicalService geographicalService,
            IBoundingBoxProvider boundingBoxProvider)
        {
            _geographicalService = geographicalService;
            _boundingBoxProvider = boundingBoxProvider;
        }

        public ValidationResult Validate(GpsPosition position)
        {
            var boundingBox = _boundingBoxProvider.GetBoundingBox();
            var isWithinBounds = _geographicalService.IsWithinBoundary(
                position.Latitude, 
                position.Longitude, 
                boundingBox);

            return !isWithinBounds
                ? ValidationResult.Failure($"Coordinates ({position.Latitude}, {position.Longitude}) are outside the allowed area.")
                : ValidationResult.Success();
        }
    }
}
