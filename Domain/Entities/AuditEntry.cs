using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace Domain.Entities
{
	public class AuditEntry
	{
		public EntityEntry Entry { get; set; }
		public string Action { get; set; }
		public string UserId { get; set; }
		public string TableName { get; set; }
		public Dictionary<string, object> OldValues { get; set; } = new Dictionary<string, object>();
		public Dictionary<string, object> NewValues { get; set; } = new Dictionary<string, object>();
		public string IpAddress { get; set; }
		public string UserAgent { get; set; }
		public List<string> ChangedColums { get; set; } = new List<string>();
		public List<PropertyEntry> TemporaryProperties { get; set; } = new();
		public bool HasTemporaryProperties => TemporaryProperties.Any();
		public AuditEntry(EntityEntry entry) => Entry = entry;


		public AuditLog ToAuditLog()
		{
			return new AuditLog
			{
				Action = Action,
				UserId = UserId,
				EntityType = Entry.Entity.GetType().Name,
				TableName = TableName,
				OldValues = OldValues.Count == 0 ? null : JsonSerializer.Serialize(OldValues),
				NewValues = NewValues.Count == 0 ? null : JsonSerializer.Serialize(NewValues),
				AffectedColumns = ChangedColums.Count == 0 ? null : string.Join(", ", ChangedColums),
				Timestamp = DateTime.UtcNow,
				IpAddress = IpAddress,
				UserAgent = UserAgent

			};
		}

	}

}
