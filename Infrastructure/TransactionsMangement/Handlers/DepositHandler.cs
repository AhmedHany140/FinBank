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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.TransactionsMangement.Handlers
{
	public class DepositHandler : IRequestHandler<DepositCommand, Result<bool>>
	{

		private readonly IUnitOfWork unofwork;
		private readonly IMediator _mediator;
		private readonly IAccountNumberEncryptor accountNumberEncryptor;
		private readonly ApplicationDbContext _context;

		public DepositHandler(ApplicationDbContext applicationDb,IMediator mediator, IAccountNumberEncryptor accountNumberEncryptor, IUnitOfWork unitOfWork)
		{
		
			_mediator = mediator;
			this.accountNumberEncryptor = accountNumberEncryptor;
			unofwork = unitOfWork;
			_context = applicationDb;

		}

		public async Task<Result<bool>> Handle(DepositCommand request, CancellationToken cancellationToken)
		{
			var dto = request.Dto;
			if (dto == null)
				return Result<bool>.Failure(new Error("DepositHandler", "Dto can't be null"));


			if (dto.Amount <= 0)
				return Result<bool>.Failure(new Error("DepositHandler", "Amount must be greater than 0"));

			var encryptedAccountNumber = accountNumberEncryptor.Encrypt(dto.ToAccountNumber);

			var account = await _context.BankAccounts
				.Include(a => a.User)
				.FirstOrDefaultAsync(a => a.AccountNumber == encryptedAccountNumber);

			if (account == null || !account.IsActive)
				return Result<bool>.Failure(new Error("DepositHandler", "No bank account found"));

			account.Balance += dto.Amount;


			var transaction = new Transaction
			{
				Amount = dto.Amount,
				ExecutedAt = DateTime.UtcNow,
				FromAccountId = account.Id,
				ToAccountId = account.Id, 
				Description = dto.Description,
				Type = TransactionType.Deposit,
				ExecutedById = account.UserId,
				Reference = Guid.NewGuid().ToString(),
			};

			await unofwork.Transactions.AddAsync(transaction);
			var result= await unofwork.CompleteAsync();



			await _mediator.Publish(new BalanceChangedEvent(
				userId: account.UserId,
				email: account.User.Email,
				amountChanged: dto.Amount,
				newBalance: account.Balance,
				reason: "Deposit"
			));

			return Result<bool>.Success(true);
		}

	}
}
