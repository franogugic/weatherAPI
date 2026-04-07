using WeatherAPI.Application.Dtos;
using WeatherAPI.Application.Models;

namespace WeatherAPI.Application.Interfaces;

public interface IForecastPersistenceService
{
    Task<FetchWeatherForecastResponseDto> SaveForecastDataAsync(
        ForecastApiResponse apiResponse,
        Coordinates coordinates,
        short? altitude,
        CancellationToken cancellationToken = default);
}
