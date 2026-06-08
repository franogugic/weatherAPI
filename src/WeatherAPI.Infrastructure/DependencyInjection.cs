using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WeatherAPI.Application.Configuration;
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
        var userConnectionString = configuration.GetConnectionString("UserDb")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__UserDb");
        var weatherApiOptions = configuration
            .GetSection(WeatherApiOptions.SectionName)
            .Get<WeatherApiOptions>();
        var llmOptions = configuration
            .GetSection(LlmOptions.SectionName)
            .Get<LlmOptions>() ?? new LlmOptions();
        var geminiOptions = configuration
            .GetSection(GeminiOptions.SectionName)
            .Get<GeminiOptions>() ?? new GeminiOptions();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Database connection string is not configured. Set 'ConnectionStrings:WeatherDb' in configuration or 'ConnectionStrings__WeatherDb' in the environment/.env file.");
        }

        if (string.IsNullOrWhiteSpace(userConnectionString))
        {
            throw new InvalidOperationException(
                "User database connection string is not configured. Set 'ConnectionStrings:UserDb' in configuration or 'ConnectionStrings__UserDb' in the environment/.env file.");
        }

        if (weatherApiOptions is null)
        {
            throw new InvalidOperationException("WeatherApi configuration section is missing.");
        }

        services.AddDbContext<WeatherDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddDbContext<UserDbContext>(options =>
            options.UseNpgsql(userConnectionString));

        services.AddOptions<WeatherApiOptions>()
            .Bind(configuration.GetSection(WeatherApiOptions.SectionName))
            .Validate(options => !string.IsNullOrWhiteSpace(options.BaseUrl), "WeatherApi:BaseUrl is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.ForecastPath), "WeatherApi:ForecastPath is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.UserAgent), "WeatherApi:UserAgent is required.")
            .Validate(options => options.TimeoutSeconds > 0, "WeatherApi:TimeoutSeconds must be greater than 0.")
            .Validate(options => options.MaxRetryAttempts > 0, "WeatherApi:MaxRetryAttempts must be greater than 0.")
            .Validate(options => options.RetryDelayMilliseconds >= 0, "WeatherApi:RetryDelayMilliseconds must be 0 or greater.")
            .ValidateOnStart();

        services.AddOptions<AuthOptions>()
            .Bind(configuration.GetSection(AuthOptions.SectionName))
            .Validate(options => options.SessionDurationDays > 0, "Auth:SessionDurationDays must be greater than 0.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.SessionCookieName), "Auth:SessionCookieName is required.")
            .Validate(options => IsValidSameSiteMode(options.CookieSameSite), "Auth:CookieSameSite must be Strict, Lax, None, or Unspecified.")
            .ValidateOnStart();

        services.AddOptions<LlmOptions>()
            .Bind(configuration.GetSection(LlmOptions.SectionName))
            .Validate(options => IsValidLlmProvider(options.Provider), "Llm:Provider must be Gemini or None.")
            .ValidateOnStart();

        services.AddOptions<GeminiOptions>()
            .Bind(configuration.GetSection(GeminiOptions.SectionName))
            .Validate(options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _), "Gemini:BaseUrl must be an absolute URL.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Model), "Gemini:Model is required.")
            .Validate(options => options.MaxOutputTokens > 0, "Gemini:MaxOutputTokens must be greater than 0.")
            .Validate(options => options.Temperature is >= 0 and <= 2, "Gemini:Temperature must be between 0 and 2.")
            .ValidateOnStart();

        services.AddHostedService<ForecastFetchBackgroundService>();
        
        services.AddTransient<RetryOnTransientFailureHandler>();
        services.AddTransient<TimeoutPerAttemptHandler>();

        services.AddHttpClient<IWeatherForecastApiClient, WeatherForecastApiClient>(client =>
        {
            client.BaseAddress = new Uri(weatherApiOptions.BaseUrl);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(weatherApiOptions.UserAgent);
        })
            .AddHttpMessageHandler<RetryOnTransientFailureHandler>()
            .AddHttpMessageHandler<TimeoutPerAttemptHandler>();

        AddLlmClient(services, llmOptions, geminiOptions);

        services.AddScoped<IWeatherForecastService, WeatherForecastService>();
        services.AddScoped<IWeatherChatService, WeatherChatService>();
        services.AddScoped<IWeatherRuleBasedAnswerService, WeatherRuleBasedAnswerService>();
        services.AddScoped<IAdminLocationService, AdminLocationService>();
        services.AddScoped<IForecastPersistenceService, ForecastPersistenceService>();
        services.AddScoped<IForecastReferenceDataService, ForecastReferenceDataService>();
        services.AddScoped<ILocationRepository, LocationRepository>();
        services.AddScoped<IForecastRepository, ForecastRepository>();
        services.AddScoped<IWeatherChatRepository, WeatherChatRepository>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        services.AddScoped<IUserPreferenceService, UserPreferenceService>();
        services.AddScoped<IUserPreferenceRepository, UserPreferenceRepository>();
        services.AddScoped<IUserFavoriteLocationService, UserFavoriteLocationService>();
        services.AddScoped<IUserFavoriteLocationRepository, UserFavoriteLocationRepository>();
        services.AddScoped<IUserDashboardLayoutService, UserDashboardLayoutService>();
        services.AddScoped<IUserDashboardLayoutRepository, UserDashboardLayoutRepository>();

        return services;
    }

    private static void AddLlmClient(
        IServiceCollection services,
        LlmOptions llmOptions,
        GeminiOptions geminiOptions)
    {
        if (llmOptions.Provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase))
        {
            services.AddHttpClient<ILlmClient, GeminiLlmClient>(client =>
            {
                client.BaseAddress = new Uri(geminiOptions.BaseUrl);
            });

            return;
        }

        services.AddScoped<ILlmClient, DisabledLlmClient>();
    }

    private static bool IsValidLlmProvider(string provider)
    {
        return provider.Equals("Gemini", StringComparison.OrdinalIgnoreCase)
            || provider.Equals("None", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsValidSameSiteMode(string cookieSameSite)
    {
        return cookieSameSite.Equals("Strict", StringComparison.OrdinalIgnoreCase)
            || cookieSameSite.Equals("Lax", StringComparison.OrdinalIgnoreCase)
            || cookieSameSite.Equals("None", StringComparison.OrdinalIgnoreCase)
            || cookieSameSite.Equals("Unspecified", StringComparison.OrdinalIgnoreCase);
    }
}
