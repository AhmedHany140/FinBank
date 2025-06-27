using Infrastructure.AuthenticationMangement.Policymangement.Services;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace Infrastructure.AuthenticationMangement.Policymangement.Handlers
{
	public class BankAccountOwnerHandler : AuthorizationHandler<BankAccountOwnerRequirement>
	{
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly ApplicationDbContext _context;

		public BankAccountOwnerHandler(IHttpContextAccessor httpContextAccessor, ApplicationDbContext context)
		{
			_httpContextAccessor = httpContextAccessor;
			_context = context;
		}
		protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, BankAccountOwnerRequirement requirement)
		{

			var httpContext = _httpContextAccessor.HttpContext;
			var routeData = httpContext?.Request.RouteValues;

			if (routeData is null || !routeData.TryGetValue("Id", out var idObj)) // "id" is the route parameter
				return ;

			if (!Guid.TryParse(idObj?.ToString(), out var bankAccountId))
				return ;

			var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

			var isOwner = await _context.BankAccounts
				.AnyAsync(b => b.Id == bankAccountId && b.UserId == Guid.Parse(userId));

			if (isOwner)
				context.Succeed(requirement);

		
		}
	}

}
