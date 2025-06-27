using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.PagenationPattern
{
	public class PagedResult<T>
	{
		public IEnumerable<T> Items { get; set; } = new List<T>();
		public int TotalCount { get; set; }
		public int Page { get; set; }
		public int PageSize { get; set; }

		public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
	}

}
