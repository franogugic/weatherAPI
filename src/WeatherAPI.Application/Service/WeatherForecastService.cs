using WeatherAPI.Application.Dtos;
using WeatherAPI.Application.Interfaces;
using WeatherAPI.Application.Models;
using WeatherAPI.Application.Common;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Application.Service;

public class WeatherForecastService : IWeatherForecastService
{
    private readonly IWeatherForecastApiClient _weatherForecastApiClient;
    private readonly IForecastPersistenceService _forecastPersistenceService;
    private readonly IForecastRepository _forecastRepository;
    private readonly ILocationRepository _locationRepository;

    public WeatherForecastService(
        IWeatherForecastApiClient weatherForecastApiClient,
        IForecastPersistenceService forecastPersistenceService,
        IForecastRepository forecastRepository,
        ILocationRepository locationRepository
        )
    {
        _weatherForecastApiClient = weatherForecastApiClient;
        _forecastPersistenceService = forecastPersistenceService;
        _forecastRepository = forecastRepository;
        _locationRepository = locationRepository;
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
    public async Task<GetWeatherForecastResponseDto> GetWeatherForecastAsync(GetWeatherForecastRequestDto request, CancellationToken cancellationToken = default)
    {
        var hourlyResponse = await _forecastRepository.
            GetHourlyForecastsAsync(request.LocationId, request.Days, cancellationToken);

        if (hourlyResponse is null)
            return new GetWeatherForecastResponseDto();

        var metaResponse = await _forecastRepository.GetUnitsByFetchAsync(hourlyResponse.ForecastFetchId, cancellationToken);
        
        return new GetWeatherForecastResponseDto
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
    }
    
    public async Task DeleteForecastFetchAsync(DeleteForecastFetchRequestDto request, CancellationToken cancellationToken = default)
    {
        var wasDeleted = await _forecastRepository.DeleteForecastFetchAsync(request.FetchId, cancellationToken);
        if (!wasDeleted)
            throw new NotFoundException($"Forecast fetch with ID {request.FetchId} was not found.");
    }

    public async Task<List<Location?>> GetLocationsAsync(CancellationToken cancellationToken = default)
    {
        return await _locationRepository.GetLocationsAsync(cancellationToken);
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
