

namespace Infrastructure.MediatR.Behavairs.AutoCachingqueries.Interface
{
	public interface ICachableQuery
	{
		string CacheKey { get; }
		bool UseCache { get; }
		TimeSpan SlidingExpiration { get; }
	}

}
