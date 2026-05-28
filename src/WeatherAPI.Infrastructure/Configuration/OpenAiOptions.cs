namespace WeatherAPI.Infrastructure.Configuration;

public sealed class OpenAiOptions
{
    public const string SectionName = "OpenAI";

    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.openai.com/";
    public string Model { get; set; } = "gpt-4.1-mini";
    public int MaxOutputTokens { get; set; } = 600;
}
