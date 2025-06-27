using MediatR;



namespace Infrastructure.NotificationsMangement.Entity
{
	public class BalanceChangedEvent : INotification
	{
		public Guid UserId { get; }
		public string Email { get; }
		public decimal AmountChanged { get; }
		public decimal NewBalance { get; }
		public string Reason { get; }

		public BalanceChangedEvent(Guid userId, string email, decimal amountChanged, decimal newBalance, string reason)
		{
			UserId = userId;
			Email = email;
			AmountChanged = amountChanged;
			NewBalance = newBalance;
			Reason = reason;
		}
	}
}
