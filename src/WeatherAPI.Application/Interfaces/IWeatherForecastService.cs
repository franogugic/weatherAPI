using WeatherAPI.Application.Dtos;
using WeatherAPI.Application.Models;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Application.Interfaces;

public interface IWeatherForecastService
{
    Task<FetchWeatherForecastResponseDto> FetchWeatherForecastAsync(
        FetchWeatherForecastRequestDto request,
        CancellationToken cancellationToken = default);

    Task<GetWeatherForecastResponseDto> GetWeatherForecastAsync(GetWeatherForecastRequestDto request,
        CancellationToken cancellationToken = default);
    
    Task DeleteForecastFetchAsync(DeleteForecastFetchRequestDto request, CancellationToken cancellationToken = default);

    Task<List<Location?>> GetLocationsAsync(CancellationToken cancellationToken = default);

}
