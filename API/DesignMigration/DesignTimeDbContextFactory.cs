using Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace API.DesignMigration
{
	public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
	{
		private readonly IHttpContextAccessor httpContextAccessor;
		public DesignTimeDbContextFactory() { }
		public DesignTimeDbContextFactory(IHttpContextAccessor httpContextAccessor)
		{
			this.httpContextAccessor = httpContextAccessor;
		}
		public ApplicationDbContext CreateDbContext(string[] args)
		{
			var configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json")
				.Build();

			var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
			var connectionString = configuration.GetConnectionString("Constr");
			builder.UseSqlServer(connectionString); 

			return new ApplicationDbContext(builder.Options, httpContextAccessor);
		}
	}
}
