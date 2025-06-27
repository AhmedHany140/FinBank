using Application.Loging;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.IntersetsMangement.Reposatory.Services
{
	public class InterestRuleRepository : IInterestRuleRepository
	{
		private readonly ApplicationDbContext _context;

		public InterestRuleRepository(ApplicationDbContext context)
		{
			_context = context;
		}

		public Task<InterestRule?> GetActiveRuleAsync(string currency)
		{
			if (string.IsNullOrWhiteSpace(currency))
			{
				throw new ArgumentException("Currency cannot be null or empty.", nameof(currency));
			}
			return _context.InterestRules
				.Where(r => r.Active && r.Currency == currency)
				.FirstOrDefaultAsync();
		}

		public async Task AddAsync(InterestRule rule)
		{
			if (rule == null)
			{
				throw new ArgumentNullException(nameof(rule), "Interest rule cannot be null.");
			}
			await _context.InterestRules.AddAsync(rule);
	
		}
	}

}
