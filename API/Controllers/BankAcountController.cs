using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
	/// <summary>
	/// Controller for managing bank accounts.
	/// </summary>
	[Route("api/[controller]")]
	[ApiController]
	public class BankAcountController : ControllerBase
	{
		private readonly IMediator mediator;

		/// <summary>
		/// Initializes a new instance of the <see cref="BankAcountController"/> class.
		/// </summary>
		/// <param name="mediator">The mediator for handling commands and queries.</param>
		public BankAcountController(IMediator mediator)
		{
			this.mediator = mediator;
		}

		/// <summary>
		/// Creates a new bank account.
		/// </summary>
		/// <param name="bankAccountCreateDto">The data transfer object containing bank account creation details.</param>
		/// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
		[HttpPost("CreateBankAcount")]
		[Authorize]
		public async Task<IActionResult> Create(BankAccountCreateDto bankAccountCreateDto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var result = await mediator.Send(new CreateBankAccountCommand(bankAccountCreateDto));

			if (result.IsSuccess)
			{
				return Ok(result.Value);
			}
			else
			{
				return BadRequest(result.Value);
			}
		}

		/// <summary>
		/// Deletes a bank account by user ID.
		/// </summary>
		/// <param name="Id">The unique identifier of the bank account to delete.</param>
		/// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
		[HttpDelete("DeleteBankAcountByUserId/{Id}")]
		[Authorize(Policy = "CheckBankAccountOwner")]
		public async Task<IActionResult> Delete(Guid Id)
		{
			if (Id == Guid.Empty)
			{
				return BadRequest("Invalid account ID.");
			}

			var result = await mediator.Send(new DeleteBankAcountCommand(Id));

			if (result.IsSuccess)
			{
				return Ok(result.Value);
			}
			else
			{
				return BadRequest(result.Value);
			}
		}

		/// <summary>
		/// Retrieves a bank account by its unique identifier.
		/// </summary>
		/// <param name="Id">The unique identifier of the bank account.</param>
		/// <returns>An <see cref="IActionResult"/> containing the bank account details or an error message.</returns>
		[HttpGet("GetBankAcountById")]
		[Authorize]
		public async Task<IActionResult> Get(Guid Id)
		{
			if (Id == Guid.Empty)
			{
				return BadRequest("Invalid account ID.");
			}

			var result = await mediator.Send(new GetBankAcountQuery(Id));

			if (result == null)
			{
				return NotFound("Bank account not found.");
			}
			return Ok(result);
		}
	}
}
