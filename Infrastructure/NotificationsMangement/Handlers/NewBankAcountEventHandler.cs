using Application.Loging;
using Domain.Entities;
using Infrastructure.AuthenticationMangement.VerifyEmails.Interfaces;
using Infrastructure.Data;
using Infrastructure.NotificationsMangement.Entity;
using MediatR;


namespace Infrastructure.NotificationsMangement.Handlers
{
	public class NewBankAcountEventHandler : INotificationHandler<NewBankAcountEvent>
	{
		private readonly IEmailSender _notificationService;
		private readonly ApplicationDbContext _context;

		public NewBankAcountEventHandler(IEmailSender notificationService, ApplicationDbContext context)
		{
			_notificationService = notificationService;
			_context = context;
		}

		public async Task Handle(NewBankAcountEvent notification, CancellationToken cancellationToken)
		{
			if (notification == null)
			{
				LogExceptions.LogEx(new ArgumentNullException(nameof(notification)), "NewBankAcountEventHandler.Handle");
				throw new ArgumentNullException(nameof(notification), "Notification cannot be null");
			}

			if (string.IsNullOrEmpty(notification.Email))
			{
				LogExceptions.LogEx(new ArgumentException("Email cannot be null or empty."), "NewBankAcountEventHandler.Handle");
				throw new ArgumentException("Email cannot be null or empty.", nameof(notification.Email));
			}


			var message = $"Welcome,Sir {notification.Email} in FinBank \n " +
				$"Your BankAcount Number = {notification.AcountNumber} \n " +
				$"Your Balance = {notification.Balance}";


			await _notificationService.SendAsync(notification.Email, "New BankAcount", message);


			var notif = new Notification
			{
				UserId = notification.UserId,
				Title = "BankAcount",
				Message = message,
				Id = Guid.NewGuid(),
				SentAt = DateTime.UtcNow,
			};

			await _context.Notifications.AddAsync(notif, cancellationToken);
			await _context.SaveChangesAsync(cancellationToken);
		}

	
	}
}
