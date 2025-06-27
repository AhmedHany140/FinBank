using Domain.Entities;
using Hangfire;
using Hangfire.MemoryStorage;
using Infrastructure.AuthenticationMangement.Policymangement.Handlers;
using Infrastructure.AuthenticationMangement.VerifyEmails.Interfaces;
using Infrastructure.AuthenticationMangement.VerifyEmails.Service;
using Infrastructure.BankAcountsMangement.Encryption.Interfaces;
using Infrastructure.BankAcountsMangement.Reposatory.Interface;
using Infrastructure.BankAcountsMangement.Reposatory.Service;
using Infrastructure.Data;
using Infrastructure.Filters;
using Infrastructure.IntersetsMangement.AutoApplyIntersetRules.Services;
using Infrastructure.MediatR.Behavairs.AutoCachingqueries;
using Infrastructure.MediatR.Behavairs.ExeptionBehavairs;
using Infrastructure.MediatR.UsersMangement.JwtToken;
using Infrastructure.Policymangement.Services;
using Infrastructure.UnitOfWork.Interfaces;
using Infrastructure.UnitOfWork.Services;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System.Reflection;
using System.Text;

public static class ServiceContainer
{
	public static IServiceCollection AddInfrastructureService(this IServiceCollection services, IConfiguration configuration)
	{
		// Registering the DbContext with SQL Server
		services.AddDbContext<ApplicationDbContext>(options =>
		options.UseSqlServer(
			configuration.GetConnectionString("Constr"),
			sqlOptions => sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
		));


		//add identity services
		services.AddIdentity<ApplicationUser, IdentityRole<Guid>>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
		.AddDefaultTokenProviders();

		// Configure Identity options
		services.Configure<IdentityOptions>(options =>
		{
		
			options.Password.RequireDigit = true;            
			options.Password.RequiredLength = 10;            
			options.Password.RequireNonAlphanumeric = true;  
			options.Password.RequireUppercase = true;       
			options.Password.RequireLowercase = true;        
			options.Password.RequiredUniqueChars = 3;          

			
			options.User.RequireUniqueEmail = true;

			
			options.SignIn.RequireConfirmedEmail = true;     
			options.SignIn.RequireConfirmedPhoneNumber = false;
			options.SignIn.RequireConfirmedAccount = true;     

		
			options.Lockout.MaxFailedAccessAttempts = 5;     
			options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); 
			options.Lockout.AllowedForNewUsers = true;
		});

		// Register Token Service
		services.AddScoped<ITokenService, TokenService>();

		// Add JWT Validation
		services.AddJWTValidation(configuration);



		services.AddHttpContextAccessor();

		services.AddAuthorization(options =>
		{
			options.AddPolicy("CheckBankAccountOwner", policy =>
				policy.Requirements.Add(new BankAccountOwnerRequirement()));
		});

		services.AddScoped<IAuthorizationHandler, BankAccountOwnerHandler>();


		// Register HttpContextAccessor
		services.AddHttpContextAccessor();

		// add memory cache
		services.AddMemoryCache();


		// Register the global response cache filter
		services.AddScoped<GlobalResponseCacheFilter>();

		// Register the global exception filter
		services.AddScoped<GlobalExceptionFilter>();

		// Register BankAccount repository
		services.AddScoped<IBankAccountRepository, BankAcountReposatory>();

		// Register the UnitOfWork
		services.AddScoped<IUnitOfWork, UnitOfWork>();

		// Register the account number encryptor
		services.AddScoped<IAccountNumberEncryptor, AccountNumberEncryptor>();

	
		// Register Email sender and OTP service
		services.AddScoped<IEmailSender, EmailSenderService>();
		services.AddScoped<IOtpService, OtpService>();

		// Register the ApplyInterestService
		services.AddScoped<IApplyInterestService, ApplyInterestService>();

		services.AddHangfire(config =>
	      config.UseMemoryStorage()); 

		services.AddHangfireServer();

		services.AddScoped<IApplyInterestService, ApplyInterestService>();


		// Add MediatR and register all handlers
		services.AddMediatR(cfg =>
		{
			cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
		});

		// Register Behaviors
		services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));
		services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));


	
		return services; 
	}


	private static IServiceCollection AddJWTValidation(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddAuthentication(options =>
		{
			options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
		}
		)
		 .AddJwtBearer(options =>
		 {
			 var key = Encoding.UTF8.GetBytes(configuration["JWT:Key"]!);
			 string issuer = configuration["JWT:issuer"]!;
			 string audience = configuration["JWT:audience"]!;

			 options.SaveToken = true;
			 options.RequireHttpsMetadata = false;
			 options.TokenValidationParameters = new TokenValidationParameters
			 {
				 ValidateIssuer = true,
				 ValidateAudience = true,
				 //ValidateLifetime = true, 
				 //ValidateIssuerSigningKey = true,
				 ValidIssuer = issuer
				?? throw new InvalidOperationException("JWT:ValidIssuer configuration is missing."),
				 ValidAudience = audience
				?? throw new InvalidOperationException("JWT:ValidAudience configuration is missing."),
				 IssuerSigningKey = new SymmetricSecurityKey(key
				?? throw new InvalidOperationException("JWT:Secret configuration is missing."))
			 };
		 });


		return services;
	}

	public static void AddConfigurationLog(string filename, string environmentName)
	{
		// Set up log directory
		var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
		Directory.CreateDirectory(logDirectory); // Ensure directory exists

		// Configure logger
		Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Debug() // Base level
			.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
			.MinimumLevel.Override("System", LogEventLevel.Warning)
			.MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
			.Enrich.FromLogContext()
			.Enrich.WithProperty("Environment", environmentName)
			.Enrich.WithMachineName()
			.Enrich.WithProcessId()
			.Enrich.WithThreadId()
			.WriteTo.Console(
				outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] " +
							   "(ReqId: {RequestId}, User: {Username}, " +
							   "Env: {Environment}) {Message:lj}{NewLine}{Exception}",
				theme: AnsiConsoleTheme.Code,
				restrictedToMinimumLevel: environmentName == "Development"
					? LogEventLevel.Debug
					: LogEventLevel.Information
			)
			.WriteTo.File(
				path: Path.Combine(logDirectory, $"{filename}-.log"),
				outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} " +
								"[{Level:u3}] (ReqId: {RequestId}, " +
								"User: {UserId}|{Username}, Role: {Role}, " +
								"Machine: {MachineName}, Thread: {ThreadId}) " +
								"{Message:lj}{NewLine}{Exception}",
				rollingInterval: RollingInterval.Day,
				retainedFileCountLimit: 30, // Keep logs for 30 days
				shared: true,
				restrictedToMinimumLevel: LogEventLevel.Information
			)
			.WriteTo.Debug(restrictedToMinimumLevel: LogEventLevel.Debug)
			.CreateLogger();

		Log.Information("Logger initialized in {Environment} environment", environmentName);
	}
}
