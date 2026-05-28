namespace WeatherAPI.Application.Interfaces;

public interface ILlmClient
{
    Task<string> GenerateAsync(
        string instructions,
        string input,
        CancellationToken cancellationToken = default);
}
