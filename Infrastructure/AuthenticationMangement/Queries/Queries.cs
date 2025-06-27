using Infrastructure.Interfaces;
using Infrastructure.ResultPattern;

public record LoginQuery(LoginDto LoginDto) : IQuery<Result<TokensDto>>;

public record RefereshTokenQuery(TokenRequestDto TokenRequestDto) : IQuery<Result<TokensDto>>;

//public record GetUserByEmailQuery(string Email) : IQuery<Result<UserDto>>, ICachableQuery
//{
//	public string CacheKey => $"GetUserByEmail_{Email}";
//	public bool UseCache => true;
//	public TimeSpan SlidingExpiration => TimeSpan.FromMinutes(5);
//};