using Application.Loging;
using Application.Mapping;
using Infrastructure.Data;
using Infrastructure.PagenationPattern;
using Infrastructure.ResultPattern;
using Infrastructure.ScheduledInterestMangement.Queries;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.ScheduledInterestMangement.Handlers
{
	public class ScheduledInterestHandler : IRequestHandler<GetAllScheduledInterestQuery, Result<PagedResult<ScheduledInterestReadDto>>>
	{
		private readonly ApplicationDbContext _context;

		public ScheduledInterestHandler(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<Result<PagedResult<ScheduledInterestReadDto>>> Handle(GetAllScheduledInterestQuery request, CancellationToken cancellationToken)
		{
			var query = _context.ScheduledInterests
				.Include(s => s.BankAccount)
				.Include(s => s.InterestRule)
				.AsNoTracking();

			var totalCount = await query.CountAsync(cancellationToken);

			var data = await query
				.OrderByDescending(x => x.AppliedAt)
				.Skip((request.Dto.page - 1) * request.Dto.pagesize)
				.Take(request.Dto.pagesize)
				.Select(x => new ScheduledInterestReadDto(
					x.Id,
					x.BankAccountId,
					x.InterestRuleId,
					x.Amount,
					x.AppliedAt
				))
				.ToListAsync(cancellationToken);

			if (data == null || !data.Any())
			{
				return Result<PagedResult<ScheduledInterestReadDto>>.Success(new PagedResult<ScheduledInterestReadDto>
				{
					Items = new List<ScheduledInterestReadDto>(),
					TotalCount = 0,
					PageSize = request.Dto.pagesize,
					Page = request.Dto.page
				});
			}

			var pagedResult = new PagedResult<ScheduledInterestReadDto>
			{
				Items = data,
				TotalCount = totalCount,
				PageSize = request.Dto.pagesize,
				Page = request.Dto.page
			};

			return Result<PagedResult<ScheduledInterestReadDto>>.Success(pagedResult);
		}
	}
}
