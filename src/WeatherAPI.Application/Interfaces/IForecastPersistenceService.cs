using WeatherAPI.Application.Dtos;
using WeatherAPI.Application.Models;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Application.Interfaces;

public interface IForecastPersistenceService
{
    Task<FetchWeatherForecastResponseDto> SaveForecastDataAsync(
        ForecastApiResponse apiResponse,
        Location location,
        CancellationToken cancellationToken = default);
}
