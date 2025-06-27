using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
	public class InterestRule
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		[StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be a 3-letter ISO code.")]
		[RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Currency must be an uppercase 3-letter code (e.g., 'EGP').")]
		public string Currency { get; set; } = "EGP";

		[Required]
		[Range(0.01, 100.0, ErrorMessage = "Interest rate must be between 0.01% and 100%.")]
		public decimal InterestRate { get; set; }

		[Required]
		public CompoundingType Compounding { get; set; }

		public bool Active { get; set; } = true;

		[Required]
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public ICollection<ScheduledInterest> ScheduledInterests { get; set; } = new List<ScheduledInterest>();
	}



}
