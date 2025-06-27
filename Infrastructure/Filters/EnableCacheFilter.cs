using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using System.Text;

namespace Infrastructure.Filters
{
	public class GlobalResponseCacheFilter : IAsyncActionFilter
	{
		private readonly IMemoryCache _cache;

		public GlobalResponseCacheFilter(IMemoryCache cache)
		{
			_cache = cache;
		}

		public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			var request = context.HttpContext.Request;

			
			if (!HttpMethods.IsGet(request.Method))
			{
				await next();
				return;
			}

		
			if (request.Headers.TryGetValue("Cache-Control", out var cacheControl) &&
				cacheControl.ToString().Contains("no-cache", StringComparison.OrdinalIgnoreCase))
			{
				await next();
				return;
			}

			var cacheKey = GenerateCacheKeyFromRequest(request, context.HttpContext.User);

			if (_cache.TryGetValue(cacheKey, out var cachedResponse))
			{
				context.Result = (IActionResult)cachedResponse!;
				return;
			}

			var executedContext = await next();

	
			if (executedContext.Result is ObjectResult objectResult &&
				objectResult.StatusCode is null or >= 200 and < 300)
			{
				_cache.Set(cacheKey, objectResult, TimeSpan.FromSeconds(60));
			}
		}

		private string GenerateCacheKeyFromRequest(HttpRequest request, ClaimsPrincipal user)
		{
			var keyBuilder = new StringBuilder();

			keyBuilder.Append($"CACHE::{request.Path}");

			if (request.QueryString.HasValue)
			{
				keyBuilder.Append($"|Query={request.QueryString}");
			}

		
			var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (!string.IsNullOrWhiteSpace(userId))
			{
				keyBuilder.Append($"|User={userId}");
			}

			return keyBuilder.ToString();
		}
	}
}
