public record TransactionReadDto(
	Guid Id,
	string? FromAccount,
	string? ToAccount,
	decimal Amount,
	string Type,
	string? Description,
	string? Reference,
	DateTime ExecutedAt
);

public record DepositDto(string ToAccountNumber, decimal Amount, string? Description);
public record WithdrawDto(string FromAccountNumber, decimal Amount, string? Description);
public record TransferDto(string FromAccountNumber, Guid ToUserId  , decimal Amount, string? Description);

