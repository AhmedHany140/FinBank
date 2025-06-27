using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
	public class BankAccount
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		public Guid UserId { get; set; }    // FK to ApplicationUser

		[Required]
		[StringLength(24, MinimumLength = 8, ErrorMessage = "Account number must be between 8 and 24 characters.")]
		[RegularExpression(@"^[A-Z0-9\-]+$", ErrorMessage = "Account number must be alphanumeric and may include dashes.")]
		public string AccountNumber { get; set; } = null!;

		[Required]
		[StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be a 3-letter ISO code.")]
		[RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Currency must be uppercase 3-letter code (e.g., 'EGP').")]
		public string Currency { get; set; } = "EGP";

		[ConcurrencyCheck]
		[Range(0, double.MaxValue, ErrorMessage = "Balance cannot be negative.")]
		public decimal Balance { get; set; } = 0;

		public bool IsActive { get; set; } = true;

		[Required]
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		[Required]
		public ApplicationUser User { get; set; } = null!;

		public ICollection<Transaction> TransactionsFrom { get; set; } = new List<Transaction>();
		public ICollection<Transaction> TransactionsTo { get; set; } = new List<Transaction>();
	}


}
