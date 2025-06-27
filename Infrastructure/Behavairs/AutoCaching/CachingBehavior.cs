using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.MediatR.Behavairs.AutoCachingqueries
{
	using global::MediatR;
	using Infrastructure.MediatR.Behavairs.AutoCachingqueries.Interface;
	using MediatR;
	using Microsoft.Extensions.Caching.Memory;

	public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
		where TRequest : ICachableQuery
	{
		private readonly IMemoryCache _cache;

		public CachingBehavior(IMemoryCache cache)
		{
			_cache = cache;
		}

		public async Task<TResponse> Handle(
			TRequest request,
			RequestHandlerDelegate<TResponse> next,
			CancellationToken cancellationToken)
		{
			if (!request.UseCache)
			{
				return await next();
			}

			if (_cache.TryGetValue(request.CacheKey, out TResponse cachedResponse))
			{
				return cachedResponse!;
			}

			var response = await next();
			_cache.Set(request.CacheKey, response, request.SlidingExpiration);
			return response;
		}
	}

}
