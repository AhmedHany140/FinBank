public record ScheduledInterestReadDto(
	Guid Id,
	Guid BankAccountId,
	Guid InterestRuleId,
	decimal Amount,
	DateTime AppliedAt
);
