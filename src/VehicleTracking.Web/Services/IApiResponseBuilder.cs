using System.Collections.Generic;
using VehicleTracking.Application.Dtos;
using VehicleTracking.Web.Models;

namespace VehicleTracking.Web.Services
{
	/// <summary>
	/// Interface for building standardized API responses.
	/// Provides a consistent response format across all API endpoints.
	/// </summary>
	public interface IApiResponseBuilder
	{
		/// <summary>
		/// Creates a successful API response with data.
		/// </summary>
		/// <typeparam name="T">The type of data being returned</typeparam>
		/// <param name="data">The data to include in the response</param>
		/// <param name="message">Optional success message</param>
		/// <returns>A standardized success response</returns>
		ApiResponse<T> Success<T>(T data, string message = null);

		/// <summary>
		/// Creates a successful API response without data.
		/// </summary>
		/// <param name="message">Optional success message</param>
		/// <returns>A standardized success response</returns>
		ApiResponse<object> Success(string message = null);

		/// <summary>
		/// Creates an error API response.
		/// </summary>
		/// <param name="message">The error message</param>
		/// <returns>A standardized error response</returns>
		ApiResponse<object> Error(string message);

		/// <summary>
		/// Creates an error API response with multiple error messages.
		/// </summary>
		/// <param name="errors">A list of error messages</param>
		/// <returns>A standardized error response with multiple errors</returns>
		ApiResponse<object> Error(List<string> errors);
	}
}
