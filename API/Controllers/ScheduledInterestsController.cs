using Infrastructure.AuditlogMangement.Queries;
using Infrastructure.ScheduledInterestMangement.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
	/// <summary>
	/// Controller for managing scheduled interests. Accessible only by Admins.
	/// </summary>
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(Roles = "Admin")]
	public class ScheduledInterestsController : ControllerBase
	{
		private readonly IMediator mediator;

		/// <summary>
		/// Initializes a new instance of the <see cref="ScheduledInterestsController"/> class.
		/// </summary>
		/// <param name="mediator">The mediator for handling commands and queries.</param>
		public ScheduledInterestsController(IMediator mediator)
		{
			this.mediator = mediator;
		}

		/// <summary>
		/// Retrieves a paginated list of scheduled interests.
		/// </summary>
		/// <param name="page">The page number (must be greater than 0).</param>
		/// <param name="pagesize">The number of items per page (must be greater than 0).</param>
		/// <returns>
		/// An <see cref="IActionResult"/> containing the paginated scheduled interests if successful,
		/// or a bad request error if parameters are invalid or the operation fails.
		/// </returns>
		[HttpGet("{page:int}/{pagesize:int}")]
		public async Task<IActionResult> GetAll(int page, int pagesize)
		{
			if (page <= 0 || pagesize <= 0)
			{
				return BadRequest("invailed parms");
			}

			var dto = new PaginationDto(page, pagesize);

			var result = await mediator.Send(new GetAllScheduledInterestQuery(dto));

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
