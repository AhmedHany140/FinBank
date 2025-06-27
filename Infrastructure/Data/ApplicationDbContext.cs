using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
	{
		private readonly IHttpContextAccessor httpContextAccessor;
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
		{
			this.httpContextAccessor = httpContextAccessor;
		}

		public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
		public DbSet<Transaction> Transactions => Set<Transaction>();
		public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
		public DbSet<Notification> Notifications => Set<Notification>();
		public DbSet<InterestRule> InterestRules => Set<InterestRule>();
		public DbSet<ScheduledInterest> ScheduledInterests => Set<ScheduledInterest>();
		public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
		public DbSet<EmailVerification> EmailVerifications { get; set; }

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.Entity<BankAccount>()
				.HasIndex(a => a.AccountNumber)
				.IsUnique();

			builder.Entity<BankAccount>()
				.HasOne(a => a.User)
				.WithMany(u => u.BankAccounts)
				.HasForeignKey(a => a.UserId);

			builder.Entity<Transaction>()
				.HasOne(t => t.FromAccount)
				.WithMany(b => b.TransactionsFrom)
				.HasForeignKey(t => t.FromAccountId)
				.OnDelete(DeleteBehavior.Restrict);

			builder.Entity<Transaction>()
				.HasOne(t => t.ToAccount)
				.WithMany(b => b.TransactionsTo)
				.HasForeignKey(t => t.ToAccountId)
				.OnDelete(DeleteBehavior.Restrict);
		}

		/// <summary>
		/// Processes entity changes before saving to the database and creates audit entries for them.
		/// </summary>
		/// <returns>
		/// A list of <see cref="AuditEntry"/> objects representing the changes to be audited.
		/// </returns>
		private List<AuditEntry> OnBeforeSaveChanges()
		{
			ChangeTracker.DetectChanges();
			var auditEntries = new List<AuditEntry>();
			foreach (var entry in ChangeTracker.Entries())
			{
				if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
					continue;

				var auditEntry = new AuditEntry(entry)
				{
					Action = entry.State.ToString(),
					UserId = httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonymous",
					TableName = entry.Entity.GetType().Name,
					IpAddress = httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
					UserAgent = httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown",
				};

				foreach (var property in entry.Properties)
				{
					string propertyname = property.Metadata.Name;
					if (property.IsTemporary)
					{
						auditEntry.TemporaryProperties.Add(property);
						continue;
					}
					switch (entry.State)
					{
						case EntityState.Added:
							auditEntry.NewValues[propertyname] = property.CurrentValue;
							break;
						case EntityState.Modified:
							if (property.IsModified)
							{
								auditEntry.OldValues[propertyname] = property.OriginalValue;
								auditEntry.NewValues[propertyname] = property.CurrentValue;
								auditEntry.ChangedColums.Add(propertyname);
							}
							break;
						case EntityState.Deleted:
							auditEntry.OldValues[propertyname] = property.OriginalValue;
							break;
					}
				}
				auditEntries.Add(auditEntry);
			}
			foreach (var audit in auditEntries.Where(a => a.HasTemporaryProperties))
			{
				AuditLogs.Add(audit.ToAuditLog());
			}
			return auditEntries.Where(a => !a.HasTemporaryProperties).ToList();
		}

		/// <summary>
		/// Asynchronously saves all changes made in this context to the database,
		/// and records audit logs for the changes.
		/// </summary>
		/// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
		/// <returns>The number of state entries written to the database.</returns>
		public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			var auditEntries = OnBeforeSaveChanges();

			var result = await base.SaveChangesAsync(cancellationToken);

			await OnAfterSaveChanges(auditEntries);

			return result;
		}

		/// <summary>
		/// Processes audit entries after changes have been saved, updating temporary properties and saving audit logs.
		/// </summary>
		/// <param name="auditEntries">The list of audit entries to process.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		private Task OnAfterSaveChanges(List<AuditEntry> auditEntries)
		{
			if (auditEntries.Count == 0 || auditEntries == null)
				return Task.CompletedTask;

			foreach (var auditEntry in auditEntries)
			{
				foreach (var prop in auditEntry.TemporaryProperties)
				{
					if (prop.Metadata.IsPrimaryKey())
					{
						auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
					}
				}

				AuditLogs.Add(auditEntry.ToAuditLog());
			}
			return SaveChangesAsync();
		}
	}

}
