using WeatherAPI.Application.Dtos;
using WeatherAPI.Application.Models;

namespace WeatherAPI.Application.Interfaces;

public interface IWeatherForecastService
{
    Task<FetchWeatherForecastResponseDto> FetchWeatherForecastAsync(
        FetchWeatherForecastRequestDto request,
        CancellationToken cancellationToken = default);

    Task<GetWeatherForecastResponseDto> GetWeatherForecast(GetWeatherForecastRequestDto request,
        CancellationToken cancellationToken = default);
    
    Task DeleteForecastFetchAsync(DeleteForecastFetchRequestDto request, CancellationToken cancellationToken = default);

}
