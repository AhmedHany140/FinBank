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
	public class TransferHandler : IRequestHandler<TransferCommand, Result<bool>>
	{
		private readonly ApplicationDbContext _context;
		private readonly IMediator _mediator;
		private readonly IAccountNumberEncryptor _encryptor;
		private readonly IUnitOfWork _unitOfWork;

		public TransferHandler(ApplicationDbContext context,IUnitOfWork ofWork, IMediator mediator, IAccountNumberEncryptor encryptor)
		{
			_context = context;
			_mediator = mediator;
			_encryptor = encryptor;
			_unitOfWork = ofWork;
		}

		public async Task<Result<bool>> Handle(TransferCommand request, CancellationToken cancellationToken)
		{
			var dto = request.Dto;

			if (dto == null)
			{
				LogExceptions.LogEx(new ArgumentNullException(nameof(dto)), "TransferHandler.Handle");
				return Result<bool>.Failure(new Error("TransferHandler", "Transfer data is missing"));
			}

			if (dto.Amount <= 0)
			{
				return Result<bool>.Failure(new Error("TransferHandler", "Amount must be greater than zero"));
			}

			var encryptedFromAcc = _encryptor.Encrypt(dto.FromAccountNumber);

			// Get sender account
			var fromAccount = await _context.BankAccounts
				.Include(a => a.User)
				.FirstOrDefaultAsync(a => a.AccountNumber == encryptedFromAcc, cancellationToken);

			if (fromAccount == null || !fromAccount.IsActive)
				return Result<bool>.Failure(new Error("TransferHandler", "Sender account not found"));

			if (fromAccount.Balance < dto.Amount)
				return Result<bool>.Failure(new Error("TransferHandler", "Insufficient balance"));

			// Get recipient account
			var toAccount = await _context.BankAccounts
				.Include(a => a.User)
				.FirstOrDefaultAsync(a => a.UserId == dto.ToUserId, cancellationToken);

			if (toAccount == null || !toAccount.IsActive)
				return Result<bool>.Failure(new Error("TransferHandler", "Recipient account not found"));

			// Adjust balances
			fromAccount.Balance -= dto.Amount;
			toAccount.Balance += dto.Amount;


			var transaction = new Transaction
			{
				Amount = dto.Amount,
				ExecutedAt = DateTime.UtcNow,
				FromAccountId = fromAccount.Id,
				ToAccountId = toAccount.Id,
				Description = dto.Description,
				Type = TransactionType.Transfer,
				ExecutedById = fromAccount.UserId,
				Reference = Guid.NewGuid().ToString(),
			};

			await _unitOfWork.Transactions.AddAsync(transaction);
			var result = await _unitOfWork.CompleteAsync();

			// Send notifications to both parties
			await _mediator.Publish(new BalanceChangedEvent(
				userId: fromAccount.UserId,
				email: fromAccount.User.Email,
				amountChanged: -dto.Amount,
				newBalance: fromAccount.Balance,
				reason: "Transfer Sent"
			), cancellationToken);

			await _mediator.Publish(new BalanceChangedEvent(
				userId: toAccount.UserId,
				email: toAccount.User.Email,
				amountChanged: dto.Amount,
				newBalance: toAccount.Balance,
				reason: "Transfer Received"
			), cancellationToken);

			return Result<bool>.Success(true);
		}
	}
}
