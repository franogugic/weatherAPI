using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
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

await ApplyMigrationsWithRetryAsync(app.Services, app.Logger, app.Lifetime.ApplicationStopping);

app.UseGlobalExceptionMiddleware();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors(FrontendCorsPolicy);

app.UseAuthorization();
app.MapControllers();


app.Run();

static async Task ApplyMigrationsWithRetryAsync(
    IServiceProvider services,
    ILogger logger,
    CancellationToken cancellationToken)
{
    const int maxAttempts = 10;
    var delay = TimeSpan.FromSeconds(5);

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WeatherDbContext>();

            await dbContext.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("Database migrations applied successfully.");
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
    var finalDbContext = finalScope.ServiceProvider.GetRequiredService<WeatherDbContext>();
    await finalDbContext.Database.MigrateAsync(cancellationToken);
}
