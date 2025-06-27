using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Infrastructure.AuthenticationMangement.VerifyEmails.Interfaces;

namespace Infrastructure.AuthenticationMangement.VerifyEmails.Service
{
	public class EmailSenderService : IEmailSender
	{
		private readonly IConfiguration _configuration;

		public EmailSenderService(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public async Task SendAsync(string to, string subject, string body)
		{
			var emailSettings = _configuration.GetSection("EmailSettings");
			var smtpServer = emailSettings["SmtpServer"];
			var port = int.Parse(emailSettings["Port"]);
			var username = emailSettings["Username"];
			var password = emailSettings["Password"];
			var fromEmail = emailSettings["FromEmail"];

			using var client = new SmtpClient();
			await client.ConnectAsync(smtpServer, port, false);
			await client.AuthenticateAsync(username, password);

			var message = new MimeMessage();
			message.From.Add(MailboxAddress.Parse(fromEmail));
			message.To.Add(MailboxAddress.Parse(to));
			message.Subject = subject;
			message.Body = new TextPart("plain") { Text = body };

			await client.SendAsync(message);
			await client.DisconnectAsync(true);
		}
	}
}