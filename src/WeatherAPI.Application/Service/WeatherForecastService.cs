using WeatherAPI.Application.Dtos;
using WeatherAPI.Application.Interfaces;
using WeatherAPI.Application.Models;

namespace WeatherAPI.Application.Service;

public class WeatherForecastService : IWeatherForecastService
{
    private readonly IWeatherForecastApiClient _weatherForecastApiClient;
    
    public WeatherForecastService(IWeatherForecastApiClient weatherForecastApiClient)
    {
        _weatherForecastApiClient = weatherForecastApiClient;
    }
    
    public async Task<MetForecastResponse> FetchWeatherForecastAsync(
        FetchWeatherForecastRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var response = await _weatherForecastApiClient.FetchForecastAsync(
            request.Latitude,
            request.Longitude,
            request.Altitude,
            cancellationToken);

        return response;
    }
    
}
