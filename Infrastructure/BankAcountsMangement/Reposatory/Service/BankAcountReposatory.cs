using Application.Loging;
using Domain.Entities;
using Infrastructure.BankAcountsMangement.Encryption.Interfaces;
using Infrastructure.BankAcountsMangement.Reposatory.Interface;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.BankAcountsMangement.Reposatory.Service
{
	public class BankAcountReposatory : IBankAccountRepository
	{
		private readonly ApplicationDbContext _context;
		private readonly IAccountNumberEncryptor _accountNumberEncryptor;

		public BankAcountReposatory(ApplicationDbContext context, IAccountNumberEncryptor accountNumberEncryptor)
		{
			_context = context;
			_accountNumberEncryptor = accountNumberEncryptor;
		}

		public async Task AddAsync(BankAccount account)
		{
			if (account == null)
			{
				LogExceptions.LogEx(new ArgumentNullException(nameof(account)), "BankAcountReposatory.AddAsync");
				throw new ArgumentNullException(nameof(account), "Bank account cannot be null.");
			}
			if (account.Id == Guid.Empty)
			{
				account.Id = Guid.NewGuid();
			}
		
			if (account.UserId == Guid.Empty)
			{
				LogExceptions.LogEx(new ArgumentException("User ID cannot be empty."), "BankAcountReposatory.AddAsync");
				throw new ArgumentException("User ID cannot be empty.", nameof(account.UserId));
			}

			account.CreatedAt = DateTime.UtcNow;
			account.IsActive = true;
			var rawAccountNumber = GenerateAccountNumber();
			account.AccountNumber= _accountNumberEncryptor.Encrypt(rawAccountNumber);

			await _context.BankAccounts.AddAsync(account);
		}
		private static readonly Random _random = new Random();
		private static readonly object _lock = new object();

		private string GenerateAccountNumber()
		{
			lock (_lock) // Ensure thread safety
			{
				var number = _random.Next(10000000, 99999999);
				return $"ACC-{number}";
			}
		}

		public async Task DeleteAsync(Guid id)
		{

			var account =await _context.BankAccounts.FindAsync(id);
			if (account == null)
			{
				LogExceptions.LogEx(new KeyNotFoundException($"Bank account with ID {id} not found."), "BankAcountReposatory.DeleteAsync");
				throw new KeyNotFoundException($"Bank account with ID {id} not found.");
			}
			_context.BankAccounts.Remove(account);
		
		}

		public async Task<BankAccount?> GetByIdAsync(Guid id)
		{

			var account =await _context.BankAccounts.FindAsync(id);

			account.AccountNumber = _accountNumberEncryptor.Decrypt(account.AccountNumber);

			if (account == null)
			{
				LogExceptions.LogEx(new KeyNotFoundException($"Bank account with ID {id} not found."), "BankAcountReposatory.GetByIdAsync");
				throw new KeyNotFoundException($"Bank account with ID {id} not found.");
			}
			if (account.UserId == Guid.Empty)
			{
				LogExceptions.LogEx(new ArgumentException("User ID cannot be empty."), "BankAcountReposatory.GetByIdAsync");
				throw new ArgumentException("User ID cannot be empty.", nameof(account.UserId));
			}
			if (string.IsNullOrWhiteSpace(account.AccountNumber))
			{
				LogExceptions.LogEx(new ArgumentException("Account number cannot be empty."), "BankAcountReposatory.GetByIdAsync");
				throw new ArgumentException("Account number cannot be empty.", nameof(account.AccountNumber));
			}
			if (account.Balance < 0)
			{
				LogExceptions.LogEx(new InvalidOperationException("Account balance cannot be negative."), "BankAcountReposatory.GetByIdAsync");
				throw new InvalidOperationException("Account balance cannot be negative.");
			}
			if (!account.IsActive)
			{
				LogExceptions.LogEx(new InvalidOperationException("Account is not active."), "BankAcountReposatory.GetByIdAsync");
				throw new InvalidOperationException("Account is not active.");
			}
			if (account.CreatedAt == default)
			{
				LogExceptions.LogEx(new InvalidOperationException("Account creation date is not set."), "BankAcountReposatory.GetByIdAsync");
				throw new InvalidOperationException("Account creation date is not set.");
			}
		
			return account;
		}


	}
}
