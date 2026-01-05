using System.Collections.Generic;
using VehicleTracking.Web.Models;

namespace VehicleTracking.Web.Services
{
	/// <summary>
	/// Implementation of API response builder.
	/// Creates standardized response objects following the Single Responsibility Principle.
	/// </summary>
	public class ApiResponseBuilder : IApiResponseBuilder
	{
		/// <summary>
		/// Creates a successful API response with data.
		/// </summary>
		public ApiResponse<T> Success<T>(T data, string message = null)
		{
			return new ApiResponse<T>
			{
				Success = true,
				Data = data,
				Message = message
			};
		}

		/// <summary>
		/// Creates a successful API response without data.
		/// </summary>
		public ApiResponse<object> Success(string message = null)
		{
			return new ApiResponse<object>
			{
				Success = true,
				Message = message
			};
		}

		/// <summary>
		/// Creates an error API response with a single error message.
		/// </summary>
		public ApiResponse<object> Error(string message)
		{
			return new ApiResponse<object>
			{
				Success = false,
				Message = message
			};
		}

		/// <summary>
		/// Creates an error API response with multiple error messages.
		/// Useful for validation errors where multiple fields may be invalid.
		/// </summary>
		public ApiResponse<object> Error(List<string> errors)
		{
			return new ApiResponse<object>
			{
				Success = false,
				Errors = errors,
				Message = "One or more errors occurred."
			};
		}
	}
}
