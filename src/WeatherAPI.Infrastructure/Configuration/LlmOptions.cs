namespace WeatherAPI.Infrastructure.Configuration;

public sealed class LlmOptions
{
    public const string SectionName = "Llm";

    public string Provider { get; set; } = "Gemini";
}
