using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
public class EmailVerification
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;
    public DateTime Expiration { get; set; }
    public bool IsUsed { get; set; }

        [ForeignKey("User")]
        public Guid UserId { get; set; } = Guid.Empty;
		public ApplicationUser User { get; set; } = null!;
}



}
