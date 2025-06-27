using Serilog;
using System;
using System.Diagnostics;
using System.Text;

namespace Application.Loging
{
	public static class LogExceptions
	{
		public static void LogEx(Exception ex, string context = null, object requestData = null)
		{
			var logMessage = new StringBuilder();

			if (!string.IsNullOrEmpty(context))
			{
				logMessage.AppendLine($"Context: {context}");
			}

			logMessage.AppendLine($"Exception Type: {ex.GetType().Name}");
			logMessage.AppendLine($"Message: {ex.Message}");

			LogError(logMessage.ToString(), ex, requestData);
		}

		private static void LogError(string baseMessage, Exception ex, object requestData = null)
		{
			Log.Error(ex, "{Message}\nStack Trace: {StackTrace}", baseMessage, ex.StackTrace);
			Log.Debug("Full Exception Details: {@Exception}", ex);

			if (requestData != null)
			{
				Log.Debug("Request Data: {@RequestData}", requestData);
			}

			if (ex.InnerException != null)
			{
				Log.Debug("Inner Exception: {@InnerException}", ex.InnerException);
			}

			var activity = Activity.Current;
			if (activity != null)
			{
				Log.Debug("Activity Trace: {TraceId}/{SpanId}",
					activity.TraceId,
					activity.SpanId);
			}
		}

		public static void LogRequest<TRequest>(TRequest request)
		{
			Log.Information("Processing {RequestType}", typeof(TRequest).Name);
			Log.Debug("Request Details: {@RequestData}", request);
		}


		public static void LogWarning(string message, object data = null)
		{
			Log.Warning("Warning: {Message}", message);

			if (data != null)
			{
				Log.Debug("Warning Data: {@Data}", data);
			}
		}

		public static void LogInformation(string message, object data = null)
		{
			Log.Information("Information: {Message}", message);

			if (data != null)
			{
				Log.Debug("Information Data: {@Data}", data);
			}
		}
	}
}