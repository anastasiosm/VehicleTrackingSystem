using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using Serilog;
using VehicleTracking.Application.Dtos;

namespace VehicleTracking.Web.App_Start
{
	public class SerilogExceptionFilterAttribute : ExceptionFilterAttribute
	{
		public override void OnException(HttpActionExecutedContext context)
		{
			// Log the exception with Serilog
			Log.Error(context.Exception, "Unhandled exception in Web API: {RequestUri}", context.Request.RequestUri);

			// Return a standardized error response
			context.Response = context.Request.CreateResponse(HttpStatusCode.InternalServerError, new ApiResponse<object>
			{
				Success = false,
				Message = "An unexpected error occurred. Please try again later.",
				Errors = new System.Collections.Generic.List<string> { context.Exception.Message }
			});
		}
	}
}
