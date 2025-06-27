using Domain.Entities;
using Infrastructure.Interfaces;
using Infrastructure.PagenationPattern;
using Infrastructure.ResultPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.AuditlogMangement.Queries
{
	public record GetAllLogsQuery(PaginationDto Dto) : IQuery<Result<PagedResult<AuditLog>>>;


}
