namespace Domain.Entities
{
	public class RefreshToken
	{
		public int Id { get; set; }
		public string Token { get; set; } = string.Empty;
		public DateTime Expires { get; set; }
		public DateTime Created { get; set; }
		public Guid UserId { get; set; }
		public bool IsRevoked { get; set; } = false;

		public ApplicationUser User { get; set; }
	}

}
