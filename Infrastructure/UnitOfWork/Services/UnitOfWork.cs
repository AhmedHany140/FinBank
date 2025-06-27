using Application.Loging;
using Infrastructure.BankAcountsMangement.Encryption.Interfaces;
using Infrastructure.BankAcountsMangement.Reposatory;
using Infrastructure.BankAcountsMangement.Reposatory.Interface;
using Infrastructure.BankAcountsMangement.Reposatory.Service;
using Infrastructure.Data;
using Infrastructure.IntersetsMangement.Reposatory.Services;
using Infrastructure.TransactionsMangement.Reposatory.Interfaces;
using Infrastructure.TransactionsMangement.Reposatory.Services;
using Infrastructure.UnitOfWork.Interfaces;


namespace Infrastructure.UnitOfWork.Services
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly ApplicationDbContext _context;

		public IBankAccountRepository BankAccounts { get; }
		public ITransactionRepository Transactions { get; }
		public IInterestRuleRepository Rules { get; }	

		public UnitOfWork(ApplicationDbContext context,IAccountNumberEncryptor accountNumberEncryptor )
		{
			_context = context;
			BankAccounts = new BankAcountReposatory(context, accountNumberEncryptor);
			Transactions = new TransactionReposatory(context);
			Rules = new InterestRuleRepository(context);
		}

		public async Task<int> CompleteAsync()
		{
			var result = 0;
			try
			{
				result = await _context.SaveChangesAsync();
			}
			catch(Exception ex)
			{

				LogExceptions.LogEx(ex, "UnitOfWork.CompleteAsync");
			}

			return result;
		}
	}

}
