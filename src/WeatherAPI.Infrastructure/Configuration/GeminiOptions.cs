namespace WeatherAPI.Infrastructure.Configuration;

public sealed class GeminiOptions
{
    public const string SectionName = "Gemini";

    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/";
    public string Model { get; set; } = "gemini-2.5-flash-lite";
    public int MaxOutputTokens { get; set; } = 600;
    public double Temperature { get; set; } = 0.4;
    public int MaxRetryAttempts { get; set; } = 2;
    public int RetryDelayMilliseconds { get; set; } = 700;
}
