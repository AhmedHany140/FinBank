using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.MediatR.UsersMangement.JwtToken
{
	public interface ITokenService
	{
		Task< string> CreateAccessToken(ApplicationUser user);
		Task< string> GenerateRefreshToken();
	}

}
