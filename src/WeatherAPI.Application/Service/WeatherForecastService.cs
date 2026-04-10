using WeatherAPI.Application.Dtos;
using WeatherAPI.Application.Interfaces;
using WeatherAPI.Application.Models;
using WeatherAPI.Application.Common;

namespace WeatherAPI.Application.Service;

public class WeatherForecastService : IWeatherForecastService
{
    private readonly IWeatherForecastApiClient _weatherForecastApiClient;
    private readonly IForecastPersistenceService _forecastPersistenceService;

    public WeatherForecastService(
        IWeatherForecastApiClient weatherForecastApiClient,
        IForecastPersistenceService forecastPersistenceService)
    {
        _weatherForecastApiClient = weatherForecastApiClient;
        _forecastPersistenceService = forecastPersistenceService;
    }

    public async Task<FetchWeatherForecastResponseDto> FetchWeatherForecastAsync(
        FetchWeatherForecastRequestDto request,
        CancellationToken cancellationToken = default)
    {
        //pripremanje kooordianta
        var coordinates = ValidateAndNormalizeCoordinates(request);
 
        // fetch s MET API-a
        var apiResponse = await _weatherForecastApiClient.FetchForecastAsync(
            coordinates.Latitude,
            coordinates.Longitude,
            request.Altitude,
            cancellationToken);

        // spremanje dohvacenih podataka u nasu bazu
        return await _forecastPersistenceService.SaveForecastDataAsync(
            apiResponse,
            coordinates,
            request.Altitude,
            cancellationToken);
    }

    private static Coordinates ValidateAndNormalizeCoordinates(FetchWeatherForecastRequestDto request)
    {
        if (request.Latitude is null || request.Longitude is null)
        {
            throw new BadRequestException("Latitude and longitude are required.");
        }

        return new Coordinates(
            Math.Round(request.Latitude.Value, 6, MidpointRounding.AwayFromZero),
            Math.Round(request.Longitude.Value, 6, MidpointRounding.AwayFromZero));
    }
}
