using Infrastructure.BankAcountsMangement.Reposatory.Interface;
using Infrastructure.TransactionsMangement.Reposatory.Interfaces;

namespace Infrastructure.UnitOfWork.Interfaces
{
	public interface IUnitOfWork
	{
		IBankAccountRepository BankAccounts { get; }
		ITransactionRepository Transactions { get; }
		IInterestRuleRepository Rules { get; }
		Task<int> CompleteAsync();
	}

}
