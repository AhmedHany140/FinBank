
public class TokenRequestDto
{
	public string RefreshToken { get; set; }
}

public record TokensDto(string AccessToken, string RefreshToken);