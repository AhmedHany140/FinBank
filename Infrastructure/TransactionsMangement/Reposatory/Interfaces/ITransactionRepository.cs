
using Domain.Entities;

namespace Infrastructure.TransactionsMangement.Reposatory.Interfaces
{
	public interface ITransactionRepository
	{
		Task AddAsync(Transaction transaction);
		Task<Transaction?> GetByIdAsync(Guid id);
	}

}
