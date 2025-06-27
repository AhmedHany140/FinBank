using Application.Loging;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.MediatR.UsersMangement.JwtToken
{
	public  class TokenService : ITokenService
	{
		private readonly IConfiguration _config;
		private readonly UserManager<ApplicationUser> userManager;

		public TokenService(IConfiguration config, UserManager<ApplicationUser> userManager)
		{
			_config = config;
			this.userManager = userManager;
		}

		public async Task< string> CreateAccessToken(ApplicationUser user)
		{
			if(user == null)
			{
				LogExceptions.LogWarning("invailed user");
				throw new ArgumentNullException(nameof(user), "User cannot be null");
			}



			var claims = new List<Claim>
			{
			new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
			new Claim(ClaimTypes.Email, user.Email)
			};

			var roles =await  userManager.GetRolesAsync(user);
			claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);



			var token = new JwtSecurityToken(
				issuer: _config["JWT:Issuer"],
				audience: _config["JWT:Audience"],
				claims: claims,
				expires: DateTime.UtcNow.AddMinutes(15),
				signingCredentials: creds
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}

		public async Task< string> GenerateRefreshToken()
		{
			var randomBytes = new byte[64];
			using var rng = RandomNumberGenerator.Create();
			rng.GetBytes(randomBytes);
			return Convert.ToBase64String(randomBytes);
		}
	}

}
