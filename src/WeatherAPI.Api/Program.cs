using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using WeatherAPI.Api.Common;
using WeatherAPI.Api.Middleware;
using WeatherAPI.Infrastructure.Configuration;
using WeatherAPI.Infrastructure;
using WeatherAPI.Infrastructure.Persistence;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

EnvironmentLoader.LoadFromRoot();


var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Model.Validation", LogLevel.Error);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();

const string FrontendCorsPolicy = "FrontendCorsPolicy";

builder.Services.AddCors(options => 
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy.SetIsOriginAllowed(origin => !string.IsNullOrWhiteSpace(origin))
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = builder.Environment.IsDevelopment()
        ? SameSiteMode.Lax
        : SameSiteMode.None;
    options.Secure = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(modelStateEntry => modelStateEntry.Value?.Errors.Count > 0)
            .ToDictionary(
                modelStateEntry => modelStateEntry.Key,
                modelStateEntry => modelStateEntry.Value!.Errors
                    .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? "The input was invalid."
                        : error.ErrorMessage)
                    .ToArray());

        var errorResponse = new ErrorResponse
        {
            StatusCode = StatusCodes.Status400BadRequest,
            Message = "Validation failed.",
            Errors = errors
        };

        return new BadRequestObjectResult(errorResponse);
    };
});

var app = builder.Build();

LogConfiguredDatabaseName(app.Configuration, app.Logger);

await ApplyMigrationsWithRetryAsync<WeatherDbContext>(
    app.Services,
    app.Logger,
    app.Lifetime.ApplicationStopping);
await ApplyMigrationsWithRetryAsync<UserDbContext>(
    app.Services,
    app.Logger,
    app.Lifetime.ApplicationStopping);

app.UseForwardedHeaders();
app.UseGlobalExceptionMiddleware();
app.UseCookiePolicy();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(FrontendCorsPolicy);

app.UseAuthorization();
app.MapControllers();


app.Run();

static void LogConfiguredDatabaseName(IConfiguration configuration, ILogger logger)
{
    var connectionString = configuration.GetConnectionString("WeatherDb")
        ?? Environment.GetEnvironmentVariable("ConnectionStrings__WeatherDb");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        logger.LogWarning("WeatherDb connection string is not configured.");
        return;
    }

    try
    {
        var connectionStringBuilder = new DbConnectionStringBuilder
        {
            ConnectionString = connectionString
        };

        var databaseName =
            connectionStringBuilder.TryGetValue("Initial Catalog", out var initialCatalog)
                ? initialCatalog?.ToString()
                : connectionStringBuilder.TryGetValue("Database", out var database)
                    ? database?.ToString()
                    : null;

        logger.LogInformation("Using database: {DatabaseName}", databaseName ?? "unknown");
    }
    catch (Exception exception)
    {
        logger.LogWarning(exception, "Could not read database name from WeatherDb connection string.");
    }
}

static async Task ApplyMigrationsWithRetryAsync<TContext>(
    IServiceProvider services,
    ILogger logger,
    CancellationToken cancellationToken)
    where TContext : DbContext
{
    const int maxAttempts = 10;
    var delay = TimeSpan.FromSeconds(5);

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();

            await dbContext.Database.MigrateAsync(cancellationToken);
            logger.LogInformation(
                "{DbContextName} migrations applied successfully.",
                typeof(TContext).Name);
            return;
        }
        catch (Exception exception) when (attempt < maxAttempts)
        {
            logger.LogWarning(
                exception,
                "Database migration attempt {Attempt}/{MaxAttempts} failed. Retrying in {DelaySeconds} seconds.",
                attempt,
                maxAttempts,
                delay.TotalSeconds);

            await Task.Delay(delay, cancellationToken);
        }
    }

    using var finalScope = services.CreateScope();
    var finalDbContext = finalScope.ServiceProvider.GetRequiredService<TContext>();
    await finalDbContext.Database.MigrateAsync(cancellationToken);
}
