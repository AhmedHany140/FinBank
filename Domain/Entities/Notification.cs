using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
	public class Notification
	{
		[Key]
		public Guid Id { get; set; }

		[Required]
		public Guid UserId { get; set; }

		[Required]
		[StringLength(100, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 100 characters.")]
		public string Title { get; set; } = null!;

		[Required]
		[StringLength(1000, MinimumLength = 5, ErrorMessage = "Message must be between 5 and 1000 characters.")]
		public string Message { get; set; } = null!;

		public bool IsRead { get; set; } = false;

		[Required]
		public DateTime SentAt { get; set; } = DateTime.UtcNow;

		[Required]
		public ApplicationUser User { get; set; } = null!;
	}

}
