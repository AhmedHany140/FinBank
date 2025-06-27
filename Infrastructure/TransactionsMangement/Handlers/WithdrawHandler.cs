

using Application.Loging;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.BankAcountsMangement.Encryption.Interfaces;
using Infrastructure.Data;
using Infrastructure.NotificationsMangement.Entity;
using Infrastructure.ResultPattern;
using Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.TransactionsMangement.Handlers
{
	public class WithdrawHandler : IRequestHandler<WithdrawCommand, Result<bool>>
	{
		private readonly ApplicationDbContext _context;
		private readonly IMediator _mediator;
		private readonly IAccountNumberEncryptor _accountNumberEncryptor;
		private readonly IUnitOfWork unofwork;

		public WithdrawHandler(ApplicationDbContext context,IUnitOfWork unitOf, IMediator mediator, IAccountNumberEncryptor accountNumberEncryptor)
		{
			_context = context;
			_mediator = mediator;
			_accountNumberEncryptor = accountNumberEncryptor;
			unofwork = unitOf;
		}

		public async Task<Result<bool>> Handle(WithdrawCommand request, CancellationToken cancellationToken)
		{
			var dto = request.Dto;
			if (dto == null)
			{
				LogExceptions.LogEx(new ArgumentNullException(nameof(dto)), "WithdrawHandler.Handle");
				return Result<bool>.Failure(new Error("WithdrawHandler", "Withdraw data is missing"));
			}

			if (dto.Amount <= 0)
			{
				LogExceptions.LogWarning("Invalid withdraw amount.");
				return Result<bool>.Failure(new Error("WithdrawHandler", "Withdraw amount must be greater than 0"));
			}

			var encryptedAccountNumber = _accountNumberEncryptor.Encrypt(dto.FromAccountNumber);

			var account = await _context.BankAccounts
				.Include(a => a.User)
				.FirstOrDefaultAsync(a => a.AccountNumber == encryptedAccountNumber);

			if (account == null || !account.IsActive)
			{
				LogExceptions.LogWarning("Bank account not found.");
				return Result<bool>.Failure(new Error("WithdrawHandler", "Bank account not found"));
			}

			if (account.Balance < dto.Amount)
			{
				LogExceptions.LogWarning("Insufficient balance for withdrawal.");
				return Result<bool>.Failure(new Error("WithdrawHandler", "Insufficient balance"));
			}

			account.Balance -= dto.Amount;

			var transaction = new Transaction
			{
				Amount = dto.Amount,
				ExecutedAt = DateTime.UtcNow,
				FromAccountId = account.Id,
				ToAccountId = account.Id,
				Description = dto.Description,
				Type = TransactionType.Withdraw,
				ExecutedById = account.UserId,
				Reference = Guid.NewGuid().ToString(),
			};

			await unofwork.Transactions.AddAsync(transaction);
			var result = await unofwork.CompleteAsync();

			await _mediator.Publish(new BalanceChangedEvent(
				userId: account.UserId,
				email: account.User.Email,
				amountChanged: -dto.Amount,
				newBalance: account.Balance,
				reason: "Withdraw"
			), cancellationToken);

			return Result<bool>.Success(true);
		}
	}
}
