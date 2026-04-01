using WeatherAPI.Application.Dtos;
using WeatherAPI.Application.Interfaces;
using WeatherAPI.Application.Models;
using WeatherAPI.Domain.Entities;

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
        if (request.Latitude is null || request.Longitude is null)
        {
            throw new ArgumentException("Latitude and longitude are required.");
        }

        var response = await _weatherForecastApiClient.FetchForecastAsync(
            request.Latitude.Value,
            request.Longitude.Value,
            request.Altitude,
            cancellationToken);
        
        // Mapiranje lokacije
        
        Location.Create(request.Latitude.Value, request.Longitude.Value, request.Altitude, null);
        
        return response;
    }
    
}
