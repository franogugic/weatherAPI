namespace WeatherAPI.Infrastructure.Configuration;

public sealed class WeatherApiOptions
{
    public const string SectionName = "WeatherApi";

    public string BaseUrl { get; set; } = string.Empty;
    public string ForecastPath { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 5;
    public int MaxRetryAttempts { get; set; } = 3;
    public int RetryDelayMilliseconds { get; set; } = 500;
}
