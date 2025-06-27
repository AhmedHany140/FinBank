using Hangfire;
using Infrastructure.Filters;
using Infrastructure.Midelwares;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Configure enhanced logging with environment awareness
ServiceContainer.AddConfigurationLog(
	filename: Assembly.GetExecutingAssembly().GetName().Name, // Use assembly name as log filename
	environmentName: builder.Environment.EnvironmentName
);

builder.Services.AddControllers(options =>
{

	options.Filters.Add<GlobalExceptionFilter>();
	options.Filters.Add<GlobalResponseCacheFilter>();

}).ConfigureApiBehaviorOptions(options =>
options.SuppressModelStateInvalidFilter = true
);


builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructureService(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionMiddleware>();


app.UseMiddleware<RequestLoggingMiddleware>();

app.UseHangfireDashboard();

// Configure Hangfire to use in-memory storage
RecurringJob.AddOrUpdate<IApplyInterestService>(
	"apply-interest",
	service => service.ApplyInterestAsync(),
	Cron.Daily // Run daily and let logic inside decide if interest should be applied
);

app.UseHangfireDashboard("/hangfire");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
