using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.BankAcountsMangement.Reposatory.Interface
{
	public interface IBankAccountRepository
	{
		Task AddAsync(BankAccount account);

		Task<BankAccount?> GetByIdAsync(Guid id);

		Task DeleteAsync(Guid id);
	}

}
