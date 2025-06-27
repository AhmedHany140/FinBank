
public record UserReadDto(
	Guid Id,
	string UserName,
	string Email,
	bool IsActive,
	DateTime CreatedAt
);

public record RegisterDto(
	string UserName,
	string Email,
	string Password
);

public record LoginDto(string Email,
	string Password);

public record SendOtpRequest(string Email);
public record VerifyOtpRequest(string Email, string OtpCode);



