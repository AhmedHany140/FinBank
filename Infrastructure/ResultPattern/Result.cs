using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.ResultPattern
{
	public class Result<T>
	{

		public bool IsSuccess { get; }
		public T Value { get; }
		public Error Error { get; }

		public Result(T value) { IsSuccess = true; Value = value; }
		public Result(Error error) { IsSuccess = false; Error = error; }

		public static Result<T> Success(T value) => new(value);
		public static Result<T> Failure(Error error) => new(error);
	}

	public record Error(string Code, string Message, string Details = null);
}
