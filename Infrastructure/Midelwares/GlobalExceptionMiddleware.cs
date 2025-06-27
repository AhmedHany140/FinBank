using Application.Loging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

public class GlobalExceptionMiddleware
{
	private readonly RequestDelegate _next;
	private readonly IWebHostEnvironment _env;

	public GlobalExceptionMiddleware(RequestDelegate next, IWebHostEnvironment env)
	{
		_next = next;
		_env = env;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await _next(context);
		}
		catch (Exception ex)
		{

			LogExceptions.LogEx(ex);

			context.Response.ContentType = "application/json";

	
			var statusCode = ex switch
			{
				ArgumentNullException => 400,
				UnauthorizedAccessException => 401,
				KeyNotFoundException => 404,
				TimeoutException => 408,
				_ => 500
			};

			context.Response.StatusCode = statusCode;

			var errorResponse = new
			{
				Message = _env.IsDevelopment() ? ex.Message : "UnExpected Error happen , Please Try again Later!",
				StatusCode = statusCode
			};

			await context.Response.WriteAsJsonAsync(errorResponse);
		}
	}
}
