using WeatherAPI.Application.Models;

namespace WeatherAPI.Application.Interfaces;

public interface IWeatherForecastApiClient
{
    Task<MetForecastResponse> FetchForecastAsync(
        decimal latitude,
        decimal longitude,
        short? altitude,
        CancellationToken cancellationToken = default);
}
