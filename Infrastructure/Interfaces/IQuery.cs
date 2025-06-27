using MediatR;

namespace Infrastructure.Interfaces
{
	public interface IQuery<TResponse> : IRequest<TResponse> { }
}
