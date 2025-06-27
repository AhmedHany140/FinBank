public record InterestRuleReadDto(
	Guid Id,
	string Currency,
	decimal InterestRate,
	string Compounding,
	bool Active,
	DateTime CreatedAt
);

public record InterestRuleCreateDto(
	string Currency,
	decimal InterestRate,
	string Compounding
);
