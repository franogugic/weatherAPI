using WeatherAPI.Application.Dtos;
using WeatherAPI.Application.Interfaces;
using WeatherAPI.Application.Models;
using WeatherAPI.Application.Common;

namespace WeatherAPI.Application.Service;

public class WeatherForecastService : IWeatherForecastService
{
    private readonly IWeatherForecastApiClient _weatherForecastApiClient;
    private readonly IForecastPersistenceService _forecastPersistenceService;
    private readonly IForecastRepository _forecastRepository;

    public WeatherForecastService(
        IWeatherForecastApiClient weatherForecastApiClient,
        IForecastPersistenceService forecastPersistenceService,
        IForecastRepository forecastRepository
        )
    {
        _weatherForecastApiClient = weatherForecastApiClient;
        _forecastPersistenceService = forecastPersistenceService;
        _forecastRepository = forecastRepository;
    }

    // fetch met api-a i spremanje u bazu
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

    // dphvat podataka iz baza
    public async Task<GetWeatherForecastResponseDto> GetWeatherForecast(GetWeatherForecastRequestDto request, CancellationToken cancellationToken = default)
    {
        if (request.Days < 1 || request.Days > 10)
            throw new BadRequestException("Days must be between 1 and 10.");
        if (request.LocationId <= 0)
            throw new BadRequestException("LocationId must be a positive integer.");
        
        var hourlyResponse = await _forecastRepository.
            GetHourlyForecastsAsync(request.LocationId, request.Days.Value, cancellationToken);

        if (hourlyResponse is null)
        {
            return new GetWeatherForecastResponseDto();
        }

        var metaResponse = await _forecastRepository.GetUnitByFetchAsync(hourlyResponse.ForecastFetchId, cancellationToken);
        
        var response = new GetWeatherForecastResponseDto
        {
            Items = hourlyResponse.Items,
            Meta = metaResponse.ToDictionary(
                x => x.MetricName,
                x => new GetWeatherForecastUnitMetaDto
                {
                    UnitDisplayName = x.UnitDisplayName,
                    UnitDescription = x.UnitDescription
                })
        };
        
        return response;
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
