using WeatherAPI.Application.Models;

namespace WeatherAPI.Application.Interfaces;

public interface IWeatherForecastApiClient
{
    Task<ForecastApiResponse> FetchForecastAsync(
        decimal latitude,
        decimal longitude,
        short? altitude,
        CancellationToken cancellationToken = default);
}
