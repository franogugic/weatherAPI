using WeatherAPI.Application.Dtos;

namespace WeatherAPI.Application.Interfaces;

public interface IWeatherChatRepository
{
    Task<ChatWeatherForecastContextDto?> GetForecastContextAsync(
        short locationId,
        int days,
        CancellationToken cancellationToken = default);
}
