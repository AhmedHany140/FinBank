using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.AuthenticationMangement.VerifyEmails.Interfaces
{
	public interface IEmailSender
	{
		Task SendAsync(string to, string subject, string body);
	}

}
