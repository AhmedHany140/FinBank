using Application.Loging;
using Infrastructure.ResultPattern;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;


namespace Infrastructure.MediatR.Behavairs.ExeptionBehavairs
{
	public class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
		where TRequest : IRequest<TResponse>
	{
		private readonly IWebHostEnvironment _env;

		public ExceptionHandlingBehavior(IWebHostEnvironment env)
		{
			_env = env;
		}

		public async Task<TResponse> Handle(
			TRequest request,
			RequestHandlerDelegate<TResponse> next,
			CancellationToken cancellationToken)
		{
			try
			{
				LogExceptions.LogRequest(request);
				return await next();
			}
			catch (Exception ex)
			{
				return HandleException(request, ex);
			}
		}

		private TResponse HandleException(TRequest request, Exception exception)
		{
			var requestType = typeof(TRequest).Name;


			LogExceptions.LogEx(exception, $"RequestType: {requestType}", request);

		
			if (IsResultType<TResponse>())
			{
				return CreateErrorResult(exception);
			}


			throw TransformException(exception);
		}

		private bool IsResultType<T>()
		{
			return typeof(T).IsGenericType &&
				   typeof(T).GetGenericTypeDefinition() == typeof(Result<>);
		}

		private dynamic CreateErrorResult(Exception exception)
		{
			var resultType = typeof(TResponse).GetGenericArguments()[0];
			var error = new Error(
				Code: GetErrorCode(exception),
				Message: _env.IsDevelopment() ? exception.Message : "An error occurred",
				Details: _env.IsDevelopment() ? exception.StackTrace : null);

			return Activator.CreateInstance(
				typeof(TResponse),
				error);
		}

		private Exception TransformException(Exception exception)
		{
			return exception switch
			{
				ValidationException ex => new BadRequestException(ex.Message),
				NotFoundException ex => new NotFoundException(ex.Message),
				UnauthorizedAccessException ex => new ForbiddenException(ex.Message),
				_ => new ApplicationException("An unexpected error occurred", exception)
			};
		}

		private string GetErrorCode(Exception exception)
		{
			return exception switch
			{
				ValidationException => "validation_error",
				NotFoundException => "not_found",
				UnauthorizedAccessException => "unauthorized",
				_ => "internal_error"
			};
		}
	}

	public class BadRequestException : Exception
	{
		public BadRequestException(string message) : base(message) { }
	}

	public class NotFoundException : Exception
	{
		public NotFoundException(string message) : base(message) { }
	}

	public class ForbiddenException : Exception
	{
		public ForbiddenException(string message) : base(message) { }
	}
}