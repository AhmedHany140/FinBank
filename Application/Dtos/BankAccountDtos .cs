public record BankAccountReadDto(
	Guid Id,
	string AccountNumber,
	string Currency,
	decimal Balance,
	bool IsActive,
	DateTime CreatedAt
);

public record BankAccountCreateDto(
	Guid UserId,
	string Currency,
	decimal Balance
);
