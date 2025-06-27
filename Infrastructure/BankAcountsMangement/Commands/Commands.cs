// Application/BankAccounts/Commands/CreateBankAccountCommand.cs
using Infrastructure.Interfaces;
using Infrastructure.ResultPattern;
using MediatR;

public record CreateBankAccountCommand(BankAccountCreateDto Dto) : ICommand;

public record DeleteBankAcountCommand(Guid Id) : ICommand;
