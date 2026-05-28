using WeatherAPI.Application.Dtos;

namespace WeatherAPI.Application.Interfaces;

public interface IWeatherChatService
{
    Task<ChatWeatherResponseDto> AskAsync(
        ChatWeatherRequestDto request,
        CancellationToken cancellationToken = default);
}
