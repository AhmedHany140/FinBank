using Application.Loging;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.TransactionsMangement.Reposatory.Interfaces;

namespace Infrastructure.TransactionsMangement.Reposatory.Services
{
	public class TransactionReposatory : ITransactionRepository
	{
		public readonly ApplicationDbContext _context;
		public TransactionReposatory(ApplicationDbContext context)
		{
			_context = context ?? throw new ArgumentNullException(nameof(context), "ApplicationDbContext cannot be null.");
		}
		public async Task AddAsync(Transaction transaction)
		{
		  
			if(transaction == null)
			{
				 LogExceptions.LogEx(new ArgumentNullException(nameof(transaction)), "TransactionReposatory.AddAsync");
				throw new ArgumentNullException(nameof(transaction), "Transaction cannot be null.");
			}
			if (transaction.Id == Guid.Empty)
			{
				transaction.Id = Guid.NewGuid();
			}

			await _context.Transactions.AddAsync(transaction);
		

		}

		public async Task<Transaction?> GetByIdAsync(Guid id)
		{

			if (id == Guid.Empty)
			{
				LogExceptions.LogEx(new ArgumentException("Invalid ID provided."), "TransactionReposatory.GetByIdAsync");
				throw new ArgumentException("Invalid ID provided.", nameof(id));
			}

			var transaction = await _context.Transactions.FindAsync(id);
			if (transaction == null)
			{
				LogExceptions.LogEx(new KeyNotFoundException($"Transaction with ID {id} not found."), "TransactionReposatory.GetByIdAsync");
				return null; 
			}
			return transaction;
		}
	}
}
