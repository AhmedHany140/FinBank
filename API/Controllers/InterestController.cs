using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
	/// <summary>
	/// Controller for managing interest rules. Accessible only by Admins.
	/// </summary>
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(Roles = "Admin")]
	public class InterestController : ControllerBase
	{
		private readonly IMediator mediator;

		/// <summary>
		/// Initializes a new instance of the <see cref="InterestController"/> class.
		/// </summary>
		/// <param name="mediator">The mediator for handling commands and queries.</param>
		public InterestController(IMediator mediator)
		{
			this.mediator = mediator;
		}

		/// <summary>
		/// Creates a new interest rule instance.
		/// </summary>
		/// <param name="dto">The data transfer object containing interest rule details.</param>
		/// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
		[HttpPost]
		public async Task<IActionResult> CreateInstanceRule(InterestRuleCreateDto dto)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var result = await mediator.Send(new CreateInterestRuleCommand(dto));

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
