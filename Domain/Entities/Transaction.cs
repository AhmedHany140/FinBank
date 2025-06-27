using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
	public class Transaction
	{
		[Key]
		public Guid Id { get; set; }

		// FromAccountId and ToAccountId are optional for Deposit/Withdraw, required for Transfer.
		public Guid? FromAccountId { get; set; }

		public Guid? ToAccountId { get; set; }

		[Required]
		[Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
		public decimal Amount { get; set; }

		[Required]
		public TransactionType Type { get; set; }

		[StringLength(100, ErrorMessage = "Reference cannot exceed 100 characters.")]
		public string? Reference { get; set; }

		[StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
		public string? Description { get; set; }

		[Required]
		public Guid ExecutedById { get; set; }   // FK to ApplicationUser

		[Required]
		public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

		public BankAccount? FromAccount { get; set; }
		public BankAccount? ToAccount { get; set; }

		[Required]
		public ApplicationUser ExecutedBy { get; set; } = null!;
	}



}
