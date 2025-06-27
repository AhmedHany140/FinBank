using Domain.Entities;

public interface IInterestRuleRepository
{
	Task<InterestRule?> GetActiveRuleAsync(string currency);
	Task AddAsync(InterestRule rule);
}
