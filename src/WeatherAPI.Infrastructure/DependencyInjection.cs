using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WeatherAPI.Application.Interfaces;
using WeatherAPI.Application.Service;
using WeatherAPI.Infrastructure.Configuration;
using WeatherAPI.Infrastructure.Persistence;
using WeatherAPI.Infrastructure.Repositories;
using WeatherAPI.Infrastructure.Services;

namespace WeatherAPI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("WeatherDb")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__WeatherDb");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Database connection string is not configured. Set 'ConnectionStrings:WeatherDb' in configuration or 'ConnectionStrings__WeatherDb' in the environment/.env file.");
        }

        services.AddDbContext<WeatherDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddOptions<WeatherApiOptions>()
            .Bind(configuration.GetSection(WeatherApiOptions.SectionName))
            .Validate(options => options.TimeoutSeconds > 0, "WeatherApi:TimeoutSeconds must be greater than 0.")
            .Validate(options => options.MaxRetryAttempts > 0, "WeatherApi:MaxRetryAttempts must be greater than 0.")
            .Validate(options => options.RetryDelayMilliseconds >= 0, "WeatherApi:RetryDelayMilliseconds must be 0 or greater.")
            .ValidateOnStart();

        services.AddTransient<RetryOnTransientFailureHandler>();
        services.AddTransient<TimeoutPerAttemptHandler>();

        services.AddHttpClient<IWeatherForecastApiClient, WeatherForecastApiClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.met.no/");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("WeatherAPI/1.0");
        })
            .AddHttpMessageHandler<RetryOnTransientFailureHandler>()
            .AddHttpMessageHandler<TimeoutPerAttemptHandler>();

        services.AddScoped<IWeatherForecastService, WeatherForecastService>();
        services.AddScoped<IForecastPersistenceService, ForecastPersistenceService>();
        services.AddScoped<IForecastReferenceDataService, ForecastReferenceDataService>();
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<IForecastRepository, ForecastRepository>();

        return services;
    }
}
