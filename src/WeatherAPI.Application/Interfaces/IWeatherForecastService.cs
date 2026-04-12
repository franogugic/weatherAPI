using WeatherAPI.Application.Dtos;
using WeatherAPI.Application.Models;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Application.Interfaces;

public interface IWeatherForecastService
{
    Task<FetchWeatherForecastResponseDto> FetchWeatherForecastAsync(
        FetchWeatherForecastRequestDto request,
        CancellationToken cancellationToken = default);

    Task<List<HourlyForecast>> GetWeatherForecast(GetWeatherForecastRequestDto request,
        CancellationToken cancellationToken = default);

}
