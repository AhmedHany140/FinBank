using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
	/// <summary>
	/// Controller for handling deposit, withdrawal, and transfer transactions.
	/// </summary>
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class TransactionsController : ControllerBase
	{
		private readonly IMediator _mediator;

		/// <summary>
		/// Initializes a new instance of the <see cref="TransactionsController"/> class.
		/// </summary>
		/// <param name="mediator">The mediator for handling commands and queries.</param>
		public TransactionsController(IMediator mediator)
		{
			_mediator = mediator;
		}

		/// <summary>
		/// Deposits funds into a bank account.
		/// </summary>
		/// <param name="dto">The deposit data transfer object containing deposit details.</param>
		/// <returns>
		/// An <see cref="IActionResult"/> indicating the result of the deposit operation.
		/// Returns <c>Ok</c> with the result on success, or <c>BadRequest</c> with an error message on failure.
		/// </returns>
		[HttpPost("Desposite")]
		public async Task<IActionResult> Desposite(DepositDto dto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var result = await _mediator.Send(new DepositCommand(dto));

			if (result.IsSuccess)
			{
				return Ok(result.Value);
			}
			else
			{
				return BadRequest(result.Error);
			}
		}

		/// <summary>
		/// Withdraws funds from a bank account.
		/// </summary>
		/// <param name="dto">The withdrawal data transfer object containing withdrawal details.</param>
		/// <returns>
		/// An <see cref="IActionResult"/> indicating the result of the withdrawal operation.
		/// Returns <c>Ok</c> with the result on success, or <c>BadRequest</c> with an error message on failure.
		/// </returns>
		[HttpPost("Withdraw")]
		public async Task<IActionResult> Withdraw(WithdrawDto dto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var result = await _mediator.Send(new WithdrawCommand(dto));

			if (result.IsSuccess)
			{
				return Ok(result.Value);
			}
			else
			{
				return BadRequest(result.Error);
			}
		}

		/// <summary>
		/// Transfers funds between bank accounts.
		/// </summary>
		/// <param name="dto">The transfer data transfer object containing transfer details.</param>
		/// <returns>
		/// An <see cref="IActionResult"/> indicating the result of the transfer operation.
		/// Returns <c>Ok</c> with the result on success, or <c>BadRequest</c> with an error message on failure.
		/// </returns>
		[HttpPost("Transfer")]
		public async Task<IActionResult> Transfer(TransferDto dto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var result = await _mediator.Send(new TransferCommand(dto));

			if (result.IsSuccess)
			{
				return Ok(result.Value);
			}
			else
			{
				return BadRequest(result.Error);
			}
		}
	}
}
