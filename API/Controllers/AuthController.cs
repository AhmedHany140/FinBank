using Domain.Entities;
using Infrastructure.AuthenticationMangement.VerifyEmails.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
	/// <summary>
	/// Controller responsible for authentication and user account management.
	/// </summary>
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IOtpService _otpService;
		private readonly IMediator _mediator;

		/// <summary>
		/// Initializes a new instance of the <see cref="AuthController"/> class.
		/// </summary>
		/// <param name="userManager">The user manager for handling user operations.</param>
		/// <param name="otpService">The OTP service for email verification.</param>
		/// <param name="mediator">The mediator for handling commands and queries.</param>
		public AuthController(UserManager<ApplicationUser> userManager, IOtpService otpService, IMediator mediator)
		{
			_userManager = userManager;
			_otpService = otpService;
			_mediator = mediator;
		}

		/// <summary>
		/// Registers a new user.
		/// </summary>
		/// <param name="registerDto">The registration data transfer object.</param>
		/// <returns>An <see cref="IActionResult"/> indicating the result of the registration.</returns>
		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var Result = await _mediator.Send(new RegisterCommand(registerDto));

			if (Result.IsSuccess)
			{
				return Ok(Result.Value);
			}
			else
			{
				return BadRequest(Result.Error);
			}
		}

		/// <summary>
		/// Authenticates a user and returns access and refresh tokens.
		/// </summary>
		/// <param name="loginDto">The login data transfer object.</param>
		/// <returns>An <see cref="IActionResult"/> containing tokens or an error.</returns>
		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			var result = await _mediator.Send(new LoginQuery(loginDto));

			if (result.IsSuccess)
			{
				return Ok(result.Value);
			}
			else
			{
				return Unauthorized(result.Error);
			}
		}

		/// <summary>
		/// Refreshes the access and refresh tokens using a valid refresh token.
		/// </summary>
		/// <param name="tokenRequestDto">The token request data transfer object containing the refresh token.</param>
		/// <returns>An <see cref="IActionResult"/> containing new tokens or an error.</returns>
		[HttpPost("refresh-token")]
		[Authorize]
		public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDto tokenRequestDto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			var result = await _mediator.Send(new RefereshTokenQuery(tokenRequestDto));
			if (result.IsSuccess)
			{
				return Ok(result.Value);
			}
			else
			{
				return Unauthorized(result.Error);
			}
		}

		/// <summary>
		/// Sends a one-time password (OTP) to the user's email for verification.
		/// </summary>
		/// <param name="request">The request containing the user's email.</param>
		/// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
		[HttpPost("send-otp")]
		[Authorize]
		public async Task<IActionResult> SendOtp(SendOtpRequest request)
		{
			var user = await _userManager.FindByEmailAsync(request.Email);
			if (user == null) return NotFound("User not found");

			await _otpService.SendOtpAsync(user);
			return Ok("OTP sent to email.");
		}

		/// <summary>
		/// Verifies the OTP code sent to the user's email.
		/// </summary>
		/// <param name="request">The request containing the user's email and OTP code.</param>
		/// <returns>An <see cref="IActionResult"/> indicating whether the OTP was valid.</returns>
		[HttpPost("verify-otp")]
		[Authorize]
		public async Task<IActionResult> VerifyOtp(VerifyOtpRequest request)
		{
			var result = await _otpService.VerifyOtpAsync(request.Email, request.OtpCode);
			if (!result) return BadRequest("Invalid or expired OTP");

			return Ok("Email confirmed.");
		}
	}
}
