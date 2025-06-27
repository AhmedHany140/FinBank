using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
	public class AuditLog
	{
		public int Id { get; set; }
		public string Action { get; set; }
		public string EntityType { get; set; }
		public string? UserId { get; set; }
		public string? TableName { get; set; }
		public string? OldValues { get; set; }
		public string? NewValues { get; set; }
		public DateTime Timestamp { get; set; }
		public string? IpAddress { get; set; }
		public string? UserAgent { get; set; }
		public string? AffectedColumns { get; set; }

	}

}
