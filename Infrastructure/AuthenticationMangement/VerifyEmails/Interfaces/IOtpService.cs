using Domain.Entities;

namespace Infrastructure.AuthenticationMangement.VerifyEmails.Interfaces
{
	public interface IOtpService
	{
		Task SendOtpAsync(ApplicationUser user);
		Task<bool> VerifyOtpAsync(string email, string code);
	}

}
