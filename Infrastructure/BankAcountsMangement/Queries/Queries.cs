
using Infrastructure.Interfaces;
using Infrastructure.ResultPattern;

public record GetBankAcountQuery(Guid Id) : IQuery<Result<BankAccountReadDto>>;
