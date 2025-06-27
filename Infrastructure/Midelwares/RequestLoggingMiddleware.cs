using Microsoft.AspNetCore.Http;
using Serilog.Context;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Midelwares
{
	public class RequestLoggingMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<RequestLoggingMiddleware> _logger;

		public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			var startTime = Stopwatch.GetTimestamp();
			var requestId = context.TraceIdentifier;

			// Extract user information
			var userId = context.User?.Claims?
				.FirstOrDefault(c => c.Type == "sub" || c.Type == "UserId")?
				.Value ?? "Anonymous";

			var username = context.User?.Identity?.IsAuthenticated == true
				? context.User.Identity.Name
				: "Anonymous";

			var role = context.User?.Claims?
				.FirstOrDefault(c => c.Type == "role")?
				.Value ?? "None";

			using (LogContext.PushProperty("RequestId", requestId))
			using (LogContext.PushProperty("UserId", userId))
			using (LogContext.PushProperty("Username", username))
			using (LogContext.PushProperty("Role", role))
			{
				try
				{
					_logger.LogInformation(
						"Starting request {Method} {Path} from {RemoteIpAddress}",
						context.Request.Method,
						context.Request.Path,
						context.Connection.RemoteIpAddress);

					await _next(context);

					var elapsedMs = GetElapsedMilliseconds(startTime, Stopwatch.GetTimestamp());

					_logger.LogInformation(
						"Completed request {Method} {Path} with {StatusCode} in {ElapsedMs}ms",
						context.Request.Method,
						context.Request.Path,
						context.Response.StatusCode,
						elapsedMs);
				}
				catch (Exception ex)
				{
					var elapsedMs = GetElapsedMilliseconds(startTime, Stopwatch.GetTimestamp());

					_logger.LogError(ex,
						"Request {Method} {Path} failed after {ElapsedMs}ms with error: {ErrorMessage}",
						context.Request.Method,
						context.Request.Path,
						elapsedMs,
						ex.Message);

					throw;
				}
			}
		}

		private static double GetElapsedMilliseconds(long start, long stop)
		{
			return (stop - start) * 1000 / (double)Stopwatch.Frequency;
		}
	}
}
