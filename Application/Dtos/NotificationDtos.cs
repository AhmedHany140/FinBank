public record NotificationReadDto(
	Guid Id,
	string Title,
	string Message,
	bool IsRead,
	DateTime SentAt
);

public record NotificationCreateDto(
	Guid UserId,
	string Title,
	string Message
);
