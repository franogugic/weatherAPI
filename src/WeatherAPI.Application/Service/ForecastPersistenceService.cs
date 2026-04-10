using WeatherAPI.Application.Dtos;
using WeatherAPI.Application.Enums;
using WeatherAPI.Application.Interfaces;
using WeatherAPI.Application.Models;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Application.Service;

public class ForecastPersistenceService : IForecastPersistenceService
{
    private const string ForecastResponseType = "compact";

    private readonly ILocationRepository _locationRepository;
    private readonly IForecastRepository _forecastRepository;
    private readonly IForecastReferenceDataService _forecastReferenceDataService;

    public ForecastPersistenceService(
        ILocationRepository locationRepository,
        IForecastRepository forecastRepository,
        IForecastReferenceDataService forecastReferenceDataService)
    {
        _locationRepository = locationRepository;
        _forecastRepository = forecastRepository;
        _forecastReferenceDataService = forecastReferenceDataService;
    }

    public async Task<FetchWeatherForecastResponseDto> SaveForecastDataAsync(
        ForecastApiResponse apiResponse,
        Coordinates coordinates,
        short? altitude,
        CancellationToken cancellationToken = default)
    {
        FetchWeatherForecastResponseDto? response = null;

        // transakcija za spremanje met api responsa u nasu bazu
        await _forecastRepository.ExecuteInTransactionAsync(
            async transactionCancellationToken =>
            {
                var location = await GetOrCreateLocationAsync(
                    coordinates.Latitude,
                    coordinates.Longitude,
                    altitude,
                    transactionCancellationToken);

                // ako met api nije vratio response
                if (apiResponse.ForecastResponse is null)
                {
                    // spremamo samo taj fetch i log njegov u bazu
                    await PersistFetchAttemptOnlyAsync(apiResponse, location, transactionCancellationToken);
                    response = CreateResponse(WeatherForecastFetchStatus.LoggedWithoutPayload, 0);
                    return;
                }

                var forecastResponse = apiResponse.ForecastResponse;

                // ako je updated at sad isti kao na prolsom fetchu za tu lokaciju, spremamo samo fetch i log
                if (await HasUnchangedForecastDataAsync(
                        location.Id,
                        forecastResponse.Properties.Meta.UpdatedAt,
                        transactionCancellationToken))
                {
                    await PersistFetchAttemptOnlyAsync(apiResponse, location, transactionCancellationToken);
                    response = CreateResponse(WeatherForecastFetchStatus.SkippedUnchanged, 0);
                    return;
                }
                
                // priprema reference pdoataka; Metric, Unit i WeatherSymbol - spremljeni u bazu
                var referenceData = await _forecastReferenceDataService.PrepareReferenceDataAsync(
                    forecastResponse,
                    transactionCancellationToken);
                
                //spremanje fetcha i lgoa
                var forecastFetch = await AddForecastFetchAndLogAsync(
                    apiResponse,
                    location,
                    transactionCancellationToken);
                
                // spremanje fetchunita u bazu, spaja metric s unitom
                await AddForecastFetchUnitsAsync(
                    forecastFetch,
                    forecastResponse.Properties.Meta.Units,
                    referenceData,
                    transactionCancellationToken);

                // kreira listu forecatFetcheva
                var hourlyForecasts = CreateHourlyForecasts(
                    forecastFetch,
                    referenceData.TimeseriesWithNextPeriod,
                    referenceData.WeatherSymbolByCode);

                await _forecastRepository.AddHourlyForecastsAsync(hourlyForecasts, transactionCancellationToken);
                //spremanje svega u bazu
                await _forecastRepository.SaveChangesAsync(transactionCancellationToken);

                response = CreateResponse(WeatherForecastFetchStatus.Persisted, hourlyForecasts.Count);
            },
            cancellationToken);

        return response ?? throw new InvalidOperationException("Forecast save flow did not produce a response.");
    }

    private async Task<Location> GetOrCreateLocationAsync(
        decimal latitude,
        decimal longitude,
        short? altitude,
        CancellationToken cancellationToken)
    {
        var location = await _locationRepository.GetLocationAsync(latitude, longitude, altitude, cancellationToken);
        if (location is not null)
        {
            return location;
        }

        location = Location.Create(latitude, longitude, altitude);
        await _locationRepository.AddAsync(location, cancellationToken);
        return location;
    }

    // ako podaci nisu promjenji od zadnjeg req za tu lokaciju
    private async Task<bool> HasUnchangedForecastDataAsync(
        short locationId,
        DateTime updatedAt,
        CancellationToken cancellationToken)
    {
        var latestFetch = await _forecastRepository.GetLatestFetchByLocationAsync(locationId, cancellationToken);
        return latestFetch is not null 
               && latestFetch.UpdatedAt == updatedAt 
               && latestFetch.FetchLog is not null
               && latestFetch.FetchLog.StatusCode == 200;
    }

    // u slucaju da met api nije vratio forecast response, ali je dohvacanje bilo uspjesno, spremamo samo fetch i log tog pokusaja
    private async Task PersistFetchAttemptOnlyAsync(
        ForecastApiResponse apiResponse,
        Location location,
        CancellationToken cancellationToken)
    {
        await AddForecastFetchAndLogAsync(apiResponse, location, cancellationToken);
        await _forecastRepository.SaveChangesAsync(cancellationToken);
    }

    //spremanje samo fetcha i loga
    private async Task<ForecastFetch> AddForecastFetchAndLogAsync(
        ForecastApiResponse apiResponse,
        Location location,
        CancellationToken cancellationToken)
    {
        var updatedAt = apiResponse.ForecastResponse?.Properties.Meta.UpdatedAt;

        var forecastFetch = ForecastFetch.Create(
            location,
            ForecastResponseType,
            updatedAt,
            DateTime.UtcNow);

        await _forecastRepository.AddForecastFetchAsync(forecastFetch, cancellationToken);

        var fetchLog = FetchLog.Create(
            forecastFetch,
            apiResponse.StatusCode,
            apiResponse.ErrorMessage);

        await _forecastRepository.AddFetchLogAsync(fetchLog, cancellationToken);

        return forecastFetch;
    }

    // spremanje fetchUniTA KOJI spaja metric s unitom
    private async Task AddForecastFetchUnitsAsync(
        ForecastFetch forecastFetch,
        IReadOnlyDictionary<string, string> units,
        ForecastReferenceData referenceData,
        CancellationToken cancellationToken)
    {
        foreach (var unitEntry in units)
        {
            var unit = referenceData.UnitByValue[unitEntry.Value];
            var metric = referenceData.MetricByName[unitEntry.Key];
            var forecastFetchUnit = ForecastFetchUnit.Create(forecastFetch, metric, unit);

            await _forecastRepository.AddForecastFetchUnitAsync(forecastFetchUnit, cancellationToken);
        }
    }

    // krieranje liste za hourlyForecast
    private static List<HourlyForecast> CreateHourlyForecasts(
        ForecastFetch forecastFetch,
        List<TimeseriesWithNextPeriod> timeseriesWithNextPeriod,
        IReadOnlyDictionary<string, WeatherSymbol> weatherSymbolByCode)
    {
        return timeseriesWithNextPeriod
            .Select(entry => CreateHourlyForecast(forecastFetch, entry, weatherSymbolByCode))
            .ToList();
    }

    // pojedinacni hourlygForecast
    private static HourlyForecast CreateHourlyForecast(
        ForecastFetch forecastFetch,
        TimeseriesWithNextPeriod entry,
        IReadOnlyDictionary<string, WeatherSymbol> weatherSymbolByCode)
    {
        var details = entry.Hourly.Data.Instant.Details;
        var weatherSymbol = ResolveWeatherSymbol(entry.NextPeriod?.Summary?.SymbolCode, weatherSymbolByCode);

        return HourlyForecast.Create(
            forecastFetch,
            entry.Hourly.Time,
            details.AirTemperature,
            details.AirPressureAtSeaLevel,
            RoundToByte(details.CloudAreaFraction),
            RoundToByte(details.RelativeHumidity),
            details.WindFromDirection,
            details.WindSpeed,
            weatherSymbol,
            entry.NextPeriod?.Details?.PrecipitationAmount);
    }

    // dohvat simbola po kodu
    private static WeatherSymbol? ResolveWeatherSymbol(
        string? symbolCode,
        IReadOnlyDictionary<string, WeatherSymbol> weatherSymbolByCode)
    {
        if (string.IsNullOrWhiteSpace(symbolCode))
        {
            return null;
        }

        return weatherSymbolByCode[symbolCode];
    }

    private static byte? RoundToByte(decimal? value)
    {
        return value is null
            ? null
            : (byte?)Math.Round(value.Value, MidpointRounding.AwayFromZero);
    }

    // kreiranje responsa
    private static FetchWeatherForecastResponseDto CreateResponse(
        WeatherForecastFetchStatus status,
        int hourlyForecastCount)
    {
        return new FetchWeatherForecastResponseDto
        {
            Status = status,
            HourlyForecastCount = hourlyForecastCount
        };
    }
}
