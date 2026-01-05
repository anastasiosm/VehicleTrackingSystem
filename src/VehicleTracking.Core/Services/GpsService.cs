using System;
using System.Collections.Generic;
using System.Linq;
using VehicleTracking.Core.Entities;
using VehicleTracking.Core.Exceptions;
using VehicleTracking.Core.Interfaces;

namespace VehicleTracking.Core.Services
{
	/// <summary>
	/// Provides GPS position management services, including validation, submission, and retrieval of vehicle location
	/// data.
	/// </summary>
	/// <remarks>GpsService offers methods to submit single or multiple GPS positions and to retrieve position
	/// history for vehicles. All submitted positions are validated before being stored. This service is typically used in
	/// applications that require tracking and managing vehicle location data. Thread safety depends on the underlying
	/// repository implementation.</remarks>
	public class GpsService : IGpsService
	{
		private readonly IGpsPositionRepository _gpsPositionRepository;
		private readonly IGpsPositionValidator _validator;

		public GpsService(
			IGpsPositionRepository gpsPositionRepository,
			IGpsPositionValidator validator)
		{
			_gpsPositionRepository = gpsPositionRepository;
			_validator = validator;
		}

		/// <summary>
		/// Validates and submits a GPS position to the repository.
		/// </summary>
		/// <remarks>This method performs validation before storing the position. If validation fails, no data is
		/// saved and an exception is thrown. The operation is atomic; either the position is stored or an exception is
		/// raised.</remarks>
		/// <param name="position">The GPS position to validate and store. Cannot be null and must meet all validation requirements.</param>
		/// <returns>true if the position was successfully validated and stored.</returns>
		/// <exception cref="ValidationException">Thrown if the specified position fails validation.</exception>
		public bool SubmitPosition(GpsPosition position)
		{
			var validationResult = _validator.ValidatePosition(position);

			if (!validationResult.IsValid)
			{
				throw new ValidationException(string.Join("; ", validationResult.Errors));
			}

			_gpsPositionRepository.Add(position);
			_gpsPositionRepository.SaveChanges();

			return true;
		}

		/// <summary>
		/// Submits a collection of GPS positions for validation and storage in the repository.
		/// </summary>
		/// <remarks>Only positions that pass individual validation are added to the repository. Positions that fail
		/// individual validation are ignored. The method does not indicate which positions were stored or skipped.</remarks>
		/// <param name="positions">The collection of GPS positions to be validated and submitted. Cannot be null. Each position is individually
		/// validated before being added to the repository.</param>
		/// <returns>true if the submission process completes successfully.</returns>
		/// <exception cref="ValidationException">Thrown if the batch of positions fails validation. The exception message contains details about the validation
		/// errors.</exception>
		public bool SubmitPositions(IEnumerable<GpsPosition> positions)
		{
			var batchValidation = _validator.ValidateBatch(positions);
			if (!batchValidation.IsValid)
			{
				throw new ValidationException(string.Join("; ", batchValidation.Errors));
			}

			var validPositions = positions
				.Where(p => _validator.ValidatePosition(p).IsValid)
				.ToList();

			if (validPositions.Any())
			{
				_gpsPositionRepository.AddRange(validPositions);
				_gpsPositionRepository.SaveChanges();
			}

			return true;
		}

		/// <summary>
		/// Retrieves the GPS positions recorded for a specified vehicle within a given time range.
		/// </summary>
		/// <param name="vehicleId">The unique identifier of the vehicle for which positions are requested.</param>
		/// <param name="from">The start of the time range for which positions are retrieved. Positions recorded at or after this time are
		/// included.</param>
		/// <param name="to">The end of the time range for which positions are retrieved. Positions recorded before or at this time are
		/// included.</param>
		/// <returns>An enumerable collection of GPS positions for the specified vehicle within the given time range. The collection
		/// will be empty if no positions are found.</returns>
		public IEnumerable<GpsPosition> GetVehiclePositions(int vehicleId, DateTime from, DateTime to)
		{
			return _gpsPositionRepository.GetPositionsForVehicle(vehicleId, from, to);
		}

		/// <summary>
		/// Retrieves the most recent GPS position recorded for the specified vehicle.
		/// </summary>
		/// <param name="vehicleId">The unique identifier of the vehicle for which to obtain the last known GPS position. Must be a positive integer.</param>
		/// <returns>A <see cref="GpsPosition"/> representing the last recorded position of the vehicle. Returns <c>null</c> if no
		/// position data is available for the specified vehicle.</returns>
		public GpsPosition GetLastPosition(int vehicleId)
		{
			return _gpsPositionRepository.GetLastPositionForVehicle(vehicleId);
		}
	}
}