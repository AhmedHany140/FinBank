using MediatR;



namespace Infrastructure.NotificationsMangement.Entity
{
	public class NewBankAcountEvent : INotification
	{
	
		public Guid UserId { get; }
		public string Email { get; }

		public decimal Balance { get; }
		public string AcountNumber { get; }

		public NewBankAcountEvent(Guid userId, string email, decimal balance, string acountNumber)
		{
			UserId = userId;
			Email = email;
			Balance = balance;
			AcountNumber = acountNumber;
		}

	}
}
