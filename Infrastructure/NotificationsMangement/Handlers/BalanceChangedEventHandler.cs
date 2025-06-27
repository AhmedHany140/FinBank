using Application.Loging;
using Domain.Entities;
using Infrastructure.AuthenticationMangement.VerifyEmails.Interfaces;
using Infrastructure.Data;
using Infrastructure.NotificationsMangement.Entity;
using MediatR;


namespace Infrastructure.NotificationsMangement.Handlers
{

	public class BalanceChangedEventHandler : INotificationHandler<BalanceChangedEvent>
	{
		private readonly IEmailSender _notificationService;
		private readonly ApplicationDbContext _context;

		public BalanceChangedEventHandler(IEmailSender notificationService, ApplicationDbContext context)
		{
			_notificationService = notificationService;
			_context = context;
		}

		public async Task Handle(BalanceChangedEvent notification, CancellationToken cancellationToken)
		{
			if (notification == null)
			{
				LogExceptions.LogEx(new ArgumentNullException(nameof(notification)), "BalanceChangedEventHandler.Handle");
				throw new ArgumentNullException(nameof(notification), "Notification cannot be null");
			}

			if (string.IsNullOrEmpty(notification.Email))
			{
				LogExceptions.LogEx(new ArgumentException("Email cannot be null or empty."), "BalanceChangedEventHandler.Handle");
				throw new ArgumentException("Email cannot be null or empty.", nameof(notification.Email));
			}


			var message = $"Your account has been updated Reson : {notification.Reason} by {notification.AmountChanged} EGP.\nNew balance: {notification.NewBalance}";


			await _notificationService.SendAsync(notification.Email, "Balance Updated", message);


			var notif = new Notification
			{
				UserId = notification.UserId,
				Title = "Balance Changed",
				Message = message,
				Id = Guid.NewGuid(),
				SentAt = DateTime.UtcNow,
			};

			await _context.Notifications.AddAsync(notif, cancellationToken);
			await _context.SaveChangesAsync(cancellationToken);
		}
	}
}
