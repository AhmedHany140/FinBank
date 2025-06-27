

using Domain.Entities;
using Infrastructure.AuthenticationMangement.VerifyEmails.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.AuthenticationMangement.VerifyEmails.Service
{
	public class OtpService : IOtpService
	{
		private readonly ApplicationDbContext _db;
		private readonly IEmailSender _emailSender;
		private readonly UserManager<ApplicationUser> _userManager;

		public OtpService(ApplicationDbContext db, IEmailSender emailSender, UserManager<ApplicationUser> userManager)
		{
			_db = db;
			_emailSender = emailSender;
			_userManager = userManager;
		}

		public async Task SendOtpAsync(ApplicationUser user)
		{
			var code = new Random().Next(100000, 999999).ToString();

			var otp = new EmailVerification
			{
				UserId = user.Id,
				Code = code,
				Expiration = DateTime.UtcNow.AddMinutes(10),
				IsUsed = false
			};

			await _db.EmailVerifications.AddAsync(otp);
			await _db.SaveChangesAsync();

			await _emailSender.SendAsync(user.Email, "Confirm your email", $"Your OTP is: {code}");
		}

		public async Task<bool> VerifyOtpAsync(string email, string code)
		{
			var user = await _userManager.FindByEmailAsync(email);
			if (user == null) return false;

			var otp = await _db.EmailVerifications
				.FirstOrDefaultAsync(x => x.UserId == user.Id && x.Code == code && !x.IsUsed);

			if (otp == null || otp.Expiration < DateTime.UtcNow)
				return false;

			otp.IsUsed = true;
			user.EmailConfirmed = true;

			await _db.SaveChangesAsync();
			await _userManager.UpdateAsync(user);

			return true;
		}
	}


}
