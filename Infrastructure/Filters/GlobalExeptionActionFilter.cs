using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using Application.Loging;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Filters
{
	public class GlobalExceptionFilter : IExceptionFilter
	{
		public void OnException(ExceptionContext context)
		{
			var errorId = Guid.NewGuid().ToString(); // Unique tracking ID
			var exception = context.Exception;

			//  Log using your custom logging service
			LogExceptions.LogEx(exception, context: $"[GlobalExceptionFilter] ErrorId={errorId}", requestData: context.HttpContext.Request?.Path);

			//  Uniform error response
			var problemDetails = new ProblemDetails
			{
				Title = "Internal Server Error",
				Status = StatusCodes.Status500InternalServerError,
				Detail = "An unexpected error occurred. Please contact support with the provided error ID.",
				Type = "https://httpstatuses.com/500"
			};

			//  Add tracking info
			problemDetails.Detail =$"ErrorId = {errorId} & timestamp : {DateTime.UtcNow}" ;

			context.Result = new ObjectResult(problemDetails)
			{
				StatusCode = StatusCodes.Status500InternalServerError
			};

			context.ExceptionHandled = true;
		}
	}
}
