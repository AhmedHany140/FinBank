using Application.Loging;
using Application.Mapping;
using Domain.Entities;
using Infrastructure.Data;
using Infrastructure.MediatR.UsersMangement.JwtToken;
using Infrastructure.ResultPattern;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Handles user registration requests.
/// </summary>
public class Registerhandeler : IRequestHandler<RegisterCommand, Result<bool>>
{
	private readonly ApplicationDbContext appDbContext;
	private readonly UserManager<ApplicationUser> userManager;

	/// <summary>
	/// Initializes a new instance of the <see cref="Registerhandeler"/> class.
	/// </summary>
	public Registerhandeler(ApplicationDbContext appDbContext, UserManager<ApplicationUser> userManager)
	{
		this.appDbContext = appDbContext;
		this.userManager = userManager;
	}

	/// <summary>
	/// Handles the user registration process.
	/// </summary>
	/// <param name="request">The registration query containing user data.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A result indicating success or failure.</returns>
	public async Task<Result<bool>> Handle(RegisterCommand request, CancellationToken cancellationToken)
	{
		LogExceptions.LogRequest(request);

		if (request.RegisterDto == null)
		{
			return Result<bool>.Failure(new Error("400", "Invalid request data", "RegisterDto cannot be null"));
		}

		var user = Mapper.Instance.ToEntity(request.RegisterDto);

		if (user == null)
		{
			return Result<bool>.Failure(new Error("400", "Mapping error", "Failed to map RegisterDto to AppUser"));
		}

		var result = await userManager.CreateAsync(user, request.RegisterDto.Password);

		if (result.Succeeded)
		{
			LogExceptions.LogInformation("Acount Created Successfully");
			return Result<bool>.Success(true);
		}
		// Log the errors if registration fails
		var errors = string.Join(", ", result.Errors.Select(e => e.Description));
		LogExceptions.LogWarning("User registration failed", new { Errors = errors });

		return Result<bool>.Failure(new Error("400", "User registration failed", errors));
	}
}

/// <summary>
/// Handles user login requests.
/// </summary>
public class LoginHandeler : IRequestHandler<LoginQuery, Result<TokensDto>>
{
	private readonly UserManager<ApplicationUser> userManager;
	private readonly ITokenService TokenService;
	private readonly ApplicationDbContext appDbContext;

	/// <summary>
	/// Initializes a new instance of the <see cref="LoginHandeler"/> class.
	/// </summary>
	public LoginHandeler(UserManager<ApplicationUser> userManager, ITokenService tokenService, ApplicationDbContext appDbContext)
	{
		this.userManager = userManager;
		TokenService = tokenService;
		this.appDbContext = appDbContext;
	}

	/// <summary>
	/// Handles the user login process.
	/// </summary>
	/// <param name="request">The login query containing credentials.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A result containing access and refresh tokens, or an error.</returns>
	public async Task<Result<TokensDto>> Handle(LoginQuery request, CancellationToken cancellationToken)
	{
		LogExceptions.LogRequest(request);
		if (request.LoginDto == null)
		{
			return Result<TokensDto>.Failure(new Error("400", "Invalid request data", "LoginDto cannot be null"));
		}
		var user = await userManager.FindByEmailAsync(request.LoginDto.Email);
		if (user == null)
		{
			return Result<TokensDto>.Failure(new Error("404", "User not found", "No user found with the provided email"));
		}
		var isPasswordValid = await userManager.CheckPasswordAsync(user, request.LoginDto.Password);
		if (!isPasswordValid)
		{
			return Result<TokensDto>.Failure(new Error("401", "Invalid credentials", "The provided password is incorrect"));
		}

		var accesstoken = await TokenService.CreateAccessToken(user);

		if (string.IsNullOrEmpty(accesstoken))
		{
			return Result<TokensDto>.Failure(new Error("500", "Token generation failed", "Failed to generate access token"));
		}

		var refreshtoken = await TokenService.GenerateRefreshToken();

		if (string.IsNullOrEmpty(refreshtoken))
		{
			return Result<TokensDto>.Failure(new Error("500", "Refresh token generation failed", "Failed to generate refresh token"));
		}

		var refreshTokenEntity = new RefreshToken
		{
			Token = refreshtoken,
			Expires = DateTime.UtcNow.AddDays(7),
			UserId = user.Id,
			Created = DateTime.UtcNow,
		};

		await appDbContext.RefreshTokens.AddAsync(refreshTokenEntity, cancellationToken);
		await appDbContext.SaveChangesAsync(cancellationToken);

		LogExceptions.LogInformation("User logged in successfully");

		return Result<TokensDto>.Success(new TokensDto(accesstoken, refreshtoken));
	}
}

/// <summary>
/// Handles refresh token requests for generating new access and refresh tokens.
/// </summary>
public class RefreshTokenHandelel : IRequestHandler<RefereshTokenQuery, Result<TokensDto>>
{
	private readonly UserManager<ApplicationUser> userManager;
	private readonly ITokenService tokenService;
	private readonly ApplicationDbContext appDbContext;

	/// <summary>
	/// Initializes a new instance of the <see cref="RefreshTokenHandelel"/> class.
	/// </summary>
	public RefreshTokenHandelel(UserManager<ApplicationUser> userManager, ITokenService tokenService, ApplicationDbContext appDbContext)
	{
		this.userManager = userManager;
		this.tokenService = tokenService;
		this.appDbContext = appDbContext;
	}

	/// <summary>
	/// Handles the refresh token process.
	/// </summary>
	/// <param name="request">The refresh token query containing the refresh token.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>A result containing new access and refresh tokens, or an error.</returns>
	public async Task<Result<TokensDto>> Handle(RefereshTokenQuery request, CancellationToken cancellationToken)
	{
		LogExceptions.LogRequest(request);

		if (request.TokenRequestDto == null || string.IsNullOrEmpty(request.TokenRequestDto.RefreshToken))
		{
			return Result<TokensDto>.Failure(new Error("400", "Invalid request data", "RefreshToken cannot be null or empty"));
		}
		// Validate the refresh token
		var storedToken = await appDbContext.RefreshTokens
			.Include(r => r.User)
			.FirstOrDefaultAsync(rt => rt.Token == request.TokenRequestDto.RefreshToken, cancellationToken);

		if (storedToken == null || storedToken.Expires < DateTime.UtcNow || storedToken.IsRevoked)
		{
			LogExceptions.LogWarning("Invalid or expired refresh token", new { RefreshToken = request.TokenRequestDto.RefreshToken });
			return Result<TokensDto>.Failure(new Error("401", "Invalid or expired refresh token", "The provided refresh token is invalid or has expired"));
		}

		storedToken.IsRevoked = true; // Mark the token as revoked

		var newaccesstoken = await tokenService.CreateAccessToken(storedToken.User);
		if (string.IsNullOrEmpty(newaccesstoken))
		{
			return Result<TokensDto>.Failure(new Error("500", "Access token generation failed", "Failed to generate new access token"));
		}

		var newrefreshtoken = await tokenService.GenerateRefreshToken();
		if (string.IsNullOrEmpty(newrefreshtoken))
		{
			return Result<TokensDto>.Failure(new Error("500", "Refresh token generation failed", "Failed to generate new refresh token"));
		}

		var newRefreshTokenEntity = new RefreshToken
		{
			Token = newrefreshtoken,
			Expires = DateTime.UtcNow.AddDays(7),
			UserId = storedToken.UserId,
			Created = DateTime.UtcNow,
		};

		await appDbContext.RefreshTokens.AddAsync(newRefreshTokenEntity, cancellationToken);
		await appDbContext.SaveChangesAsync(cancellationToken);

		LogExceptions.LogInformation("Refresh token generated successfully");

		return Result<TokensDto>.Success(new TokensDto(newaccesstoken, newrefreshtoken));
	}
}

/// <summary>
/// Handles retrieving a user by their email address.
/// </summary>
//public class GetUserByEmailHandeler : IRequestHandler<GetUserByEmailQuery, Result<UserDto>>
//{
//	private readonly ApplicationDbContext appDbContext;

//	/// <summary>
//	/// Initializes a new instance of the <see cref="GetUserByEmailHandeler"/> class.
//	/// </summary>
//	public GetUserByEmailHandeler(ApplicationDbContext appDbContext)
//	{
//		this.appDbContext = appDbContext;
//	}

//	/// <summary>
//	/// Handles retrieving a user by their email address.
//	/// </summary>
//	/// <param name="request">The get user by email query.</param>
//	/// <param name="cancellationToken">A cancellation token.</param>
//	/// <returns>A result containing the user DTO, or an error.</returns>
//	public async Task<Result<UserDto>> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
//	{
//		LogExceptions.LogRequest(request);
//		if (string.IsNullOrEmpty(request.Email))
//		{
//			LogExceptions.LogWarning("Invalid email provided", new { Email = request.Email });
//			return Result<UserDto>.Failure(new Error("400", "Invalid email", "Email cannot be null or empty"));
//		}

//		var user = await UserCompiledQueries.GetUserByEmailQuery(appDbContext, request.Email, cancellationToken);

//		if (user == null)
//		{
//			LogExceptions.LogWarning("User not found", new { Email = request.Email });
//			return Result<UserDto>.Failure(new Error("404", "User not found", "No user found with the provided email"));
//		}

//		var userDto = await MapperController<AppUser, UserDto>.Map(user);
//		if (userDto == null)
//		{
//			LogExceptions.LogWarning("Mapping error", new { User = user });
//			return Result<UserDto>.Failure(new Error("500", "Mapping error", "Failed to map AppUser to UserDto"));
//		}

//		LogExceptions.LogInformation("User retrieved successfully", new { Email = request.Email });
//		return Result<UserDto>.Success(userDto);
//	}
//}
