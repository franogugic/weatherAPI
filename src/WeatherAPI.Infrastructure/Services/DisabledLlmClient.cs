using WeatherAPI.Application.Interfaces;

namespace WeatherAPI.Infrastructure.Services;

public class DisabledLlmClient : ILlmClient
{
    public Task<string> GenerateAsync(
        string instructions,
        string input,
        CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("LLM provider is disabled.");
    }
}
