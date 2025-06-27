using Domain.Entities;
using Infrastructure.AuditlogMangement.Queries;
using Infrastructure.Data;
using Infrastructure.PagenationPattern;
using Infrastructure.ResultPattern;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace Infrastructure.AuditlogMangement.Handlers
{
	public class AuditlogHandler : IRequestHandler<GetAllLogsQuery, Result<PagedResult<AuditLog>>>
	{
		private readonly ApplicationDbContext _context;

		public AuditlogHandler(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<Result<PagedResult<AuditLog>>> Handle(GetAllLogsQuery request, CancellationToken cancellationToken)
		{
			var query = _context.AuditLogs
				.AsNoTracking();

			var totalCount = await query.CountAsync(cancellationToken);

			var data = await query
				.OrderByDescending(x => x.Timestamp)
				.Skip((request.Dto.page - 1) * request.Dto.pagesize)
				.Take(request.Dto.pagesize)
				.ToListAsync(cancellationToken);


			if (data == null || !data.Any())
			{
				return Result<PagedResult<AuditLog>>.Success(new PagedResult<AuditLog>
				{
					Items = new List<AuditLog>(),
					TotalCount = 0,
					PageSize = request.Dto.pagesize,
					Page = request.Dto.page
				});
			}

			var pagedResult = new PagedResult<AuditLog>
			{
				Items = data,
				TotalCount = totalCount,
				PageSize = request.Dto.pagesize,
				Page = request.Dto.page
			};

			return Result<PagedResult<AuditLog>>.Success(pagedResult);
		}
	}
}
