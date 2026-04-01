using WeatherAPI.Application.Dtos;
using WeatherAPI.Application.Models;

namespace WeatherAPI.Application.Interfaces;

public interface IWeatherForecastService
{
    Task<MetForecastResponse> FetchWeatherForecastAsync(
        FetchWeatherForecastRequestDto request,
        CancellationToken cancellationToken = default);
}
