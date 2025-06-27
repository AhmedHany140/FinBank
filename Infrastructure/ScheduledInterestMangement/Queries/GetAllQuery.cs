using Infrastructure.Interfaces;
using Infrastructure.PagenationPattern;
using Infrastructure.ResultPattern;


namespace Infrastructure.ScheduledInterestMangement.Queries
{
	public record GetAllScheduledInterestQuery(PaginationDto Dto): IQuery<Result<PagedResult<ScheduledInterestReadDto>>>;

}
 