namespace Domain.Entities
{
	public class ScheduledInterest
	{
		public Guid Id { get; set; }
		public Guid BankAccountId { get; set; }
		public Guid InterestRuleId { get; set; }
		public decimal Amount { get; set; }
		public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

		public BankAccount BankAccount { get; set; } = null!;
		public InterestRule InterestRule { get; set; } = null!;
	}



}
