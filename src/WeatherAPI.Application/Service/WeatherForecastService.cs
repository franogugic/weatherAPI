using WeatherAPI.Application.Dtos;
using WeatherAPI.Application.Interfaces;
using WeatherAPI.Application.Models;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Application.Service;

public class WeatherForecastService : IWeatherForecastService
{
    private readonly IWeatherForecastApiClient _weatherForecastApiClient;
    private readonly ILocationRepository _locationRepository;
    private readonly IForecastRepository _forecastRepository;
    
    public WeatherForecastService(
        IWeatherForecastApiClient weatherForecastApiClient,
        ILocationRepository locationRepository,
        IForecastRepository forecastRepository)
    {
        _locationRepository = locationRepository;
        _weatherForecastApiClient = weatherForecastApiClient;
        _forecastRepository = forecastRepository;
    }
    
    public async Task<FetchWeatherForecastResponseDto> FetchWeatherForecastAsync(
        FetchWeatherForecastRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (request.Latitude is null || request.Longitude is null)
        {
            throw new ArgumentException("Latitude and longitude are required.");
        }

        var latitude = Math.Round(request.Latitude.Value, 6, MidpointRounding.AwayFromZero);
        var longitude = Math.Round(request.Longitude.Value, 6, MidpointRounding.AwayFromZero);

        var apiResponse = await FetchApiDataAsync(latitude, longitude, request.Altitude, cancellationToken);
        return await SaveForecastDataAsync(apiResponse, latitude, longitude, request.Altitude, cancellationToken);
    }

    private async Task<ForecastApiResponse> FetchApiDataAsync(
        decimal latitude,
        decimal longitude,
        short? altitude,
        CancellationToken cancellationToken)
    {
        // fetch podataka MET API-a
        var apiResponse = await _weatherForecastApiClient.FetchForecastAsync(
            latitude,
            longitude,
            altitude,
            cancellationToken);

        return apiResponse;
    }

    private async Task<FetchWeatherForecastResponseDto> SaveForecastDataAsync(
        ForecastApiResponse apiResponse,
        decimal latitude,
        decimal longitude,
        short? altitude,
        CancellationToken cancellationToken)
    {
        FetchWeatherForecastResponseDto? response = null;

        await _forecastRepository.ExecuteInTransactionAsync(
            async transactionCancellationToken =>
            {
                var location = await GetOrCreateLocationAsync(
                    latitude,
                    longitude,
                    altitude,
                    transactionCancellationToken);

                if (apiResponse.ForecastResponse is null)
                {
                    await PersistFetchAttemptOnlyAsync(
                        apiResponse,
                        location,
                        transactionCancellationToken);
                    response = new FetchWeatherForecastResponseDto
                    {
                        Message = "Forecast fetch attempt was logged, but no forecast payload was returned.",
                        ForecastDataPersisted = false,
                        SkippedBecauseForecastUnchanged = false,
                        HourlyForecastCount = 0
                    };
                    return;
                }

                if (await HasUnchangedForecastDataAsync(
                        location.Id,
                        apiResponse.ForecastResponse.Properties.Meta.UpdatedAt,
                        transactionCancellationToken))
                {
                    await PersistFetchAttemptOnlyAsync(
                        apiResponse,
                        location,
                        transactionCancellationToken);
                    response = new FetchWeatherForecastResponseDto
                    {
                        Message = "Forecast fetch was logged, but forecast data was unchanged and was not persisted again.",
                        ForecastDataPersisted = false,
                        SkippedBecauseForecastUnchanged = true,
                        HourlyForecastCount = 0
                    };
                    return;
                }

                var lookupData = await PrepareLookupDataAsync(
                    apiResponse.ForecastResponse,
                    transactionCancellationToken);

                var forecastFetch = await AddForecastFetchAndLogAsync(
                    apiResponse,
                    location,
                    transactionCancellationToken);

                foreach (var fetchUnit in apiResponse.ForecastResponse.Properties.Meta.Units)
                {
                    var unit = lookupData.UnitByValue[fetchUnit.Value];
                    var metric = lookupData.MetricByName[fetchUnit.Key];

                    var forecastFetchUnit = ForecastFetchUnit.Create(forecastFetch, metric, unit);
                    await _forecastRepository.AddForecastFetchUnitAsync(forecastFetchUnit, transactionCancellationToken);
                }

                var hourlyForecasts = CreateHourlyForecasts(
                    forecastFetch,
                    lookupData.TimeseriesWithNextPeriod,
                    lookupData.WeatherSymbolByCode);

                await _forecastRepository.AddHourlyForecastsAsync(hourlyForecasts, transactionCancellationToken);
                await _forecastRepository.SaveChangesAsync(transactionCancellationToken);
                response = new FetchWeatherForecastResponseDto
                {
                    Message = "Forecast data fetched and persisted successfully.",
                    ForecastDataPersisted = true,
                    SkippedBecauseForecastUnchanged = false,
                    HourlyForecastCount = hourlyForecasts.Count
                };
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
        await _forecastRepository.SaveChangesAsync(cancellationToken);
        return location;
    }

    private async Task<LookupPreparation> PrepareLookupDataAsync(
        MetForecastResponse forecastResponse,
        CancellationToken cancellationToken)
    {
        var unitByValue = (await _forecastRepository.GetUnitsByValuesAsync(
                forecastResponse.Properties.Meta.Units.Values,
                cancellationToken))
            .ToDictionary(unit => unit.Value);

        var metricByName = (await _forecastRepository.GetMetricsByNamesAsync(
                forecastResponse.Properties.Meta.Units.Keys,
                cancellationToken))
            .ToDictionary(metric => metric.Name);

        var timeseriesWithNextPeriod = forecastResponse.Properties.Timeseries
            .Select(hourly => new TimeseriesWithNextPeriod(
                hourly,
                hourly.Data.Next1Hours
                ?? hourly.Data.Next6Hours
                ?? hourly.Data.Next12Hours))
            .ToList();

        var symbolCodes = timeseriesWithNextPeriod
            .Select(entry => entry.NextPeriod?.Summary?.SymbolCode)
            .Where(symbolCode => !string.IsNullOrWhiteSpace(symbolCode))
            .Cast<string>()
            .Distinct()
            .ToList();

        var weatherSymbolByCode = (await _forecastRepository.GetWeatherSymbolsByCodesAsync(
                symbolCodes,
                cancellationToken))
            .ToDictionary(symbol => symbol.Code);

        foreach (var fetchUnit in forecastResponse.Properties.Meta.Units)
        {
            if (!unitByValue.TryGetValue(fetchUnit.Value, out var unit))
            {
                unit = Unit.Create(fetchUnit.Value, fetchUnit.Value, null);
                await _forecastRepository.AddUnitAsync(unit, cancellationToken);
                unitByValue[fetchUnit.Value] = unit;
            }

            if (!metricByName.TryGetValue(fetchUnit.Key, out var metric))
            {
                metric = Metric.Create(fetchUnit.Key);
                await _forecastRepository.AddMetricAsync(metric, cancellationToken);
                metricByName[fetchUnit.Key] = metric;
            }
        }

        foreach (var timeseriesEntry in timeseriesWithNextPeriod)
        {
            var symbolCode = timeseriesEntry.NextPeriod?.Summary?.SymbolCode;

            WeatherSymbol? weatherSymbol = null;
            if (!string.IsNullOrWhiteSpace(symbolCode) && !weatherSymbolByCode.TryGetValue(symbolCode, out weatherSymbol))
            {
                weatherSymbol = WeatherSymbol.Create(symbolCode);
                await _forecastRepository.AddWeatherSymbolAsync(weatherSymbol, cancellationToken);
                weatherSymbolByCode[symbolCode] = weatherSymbol;
            }
        }

        await _forecastRepository.SaveChangesAsync(cancellationToken);

        return new LookupPreparation(
            unitByValue,
            metricByName,
            weatherSymbolByCode,
            timeseriesWithNextPeriod);
    }

    private async Task<bool> HasUnchangedForecastDataAsync(
        short locationId,
        DateTime updatedAt,
        CancellationToken cancellationToken)
    {
        var latestFetch = await _forecastRepository.GetLatestFetchByLocationAsync(locationId, cancellationToken);
        return latestFetch is not null && latestFetch.UpdatedAt == updatedAt;
    }

    private static List<HourlyForecast> CreateHourlyForecasts(
        ForecastFetch forecastFetch,
        List<TimeseriesWithNextPeriod> timeseriesWithNextPeriod,
        Dictionary<string, WeatherSymbol> weatherSymbolByCode)
    {
        var hourlyForecasts = new List<HourlyForecast>();

        foreach (var entry in timeseriesWithNextPeriod)
        {
            var symbolCode = entry.NextPeriod?.Summary?.SymbolCode;
            var cloudAreaFraction = entry.Hourly.Data.Instant.Details.CloudAreaFraction;
            var relativeHumidity = entry.Hourly.Data.Instant.Details.RelativeHumidity;

            WeatherSymbol? weatherSymbol = null;
            if (!string.IsNullOrWhiteSpace(symbolCode))
            {
                weatherSymbol = weatherSymbolByCode[symbolCode];
            }

            var hourlyForecast = HourlyForecast.Create(
                forecastFetch,
                entry.Hourly.Time,
                entry.Hourly.Data.Instant.Details.AirTemperature,
                entry.Hourly.Data.Instant.Details.AirPressureAtSeaLevel,
                cloudAreaFraction is null
                    ? null
                    : (byte?)Math.Round(
                        cloudAreaFraction.Value,
                        MidpointRounding.AwayFromZero),
                relativeHumidity is null
                    ? null
                    : (byte?)Math.Round(
                        relativeHumidity.Value,
                        MidpointRounding.AwayFromZero),
                entry.Hourly.Data.Instant.Details.WindFromDirection,
                entry.Hourly.Data.Instant.Details.WindSpeed,
                weatherSymbol,
                entry.NextPeriod?.Details?.PrecipitationAmount);

            hourlyForecasts.Add(hourlyForecast);
        }

        return hourlyForecasts;
    }

    private async Task PersistFetchAttemptOnlyAsync(
        ForecastApiResponse apiResponse,
        Location location,
        CancellationToken cancellationToken)
    {
        await AddForecastFetchAndLogAsync(apiResponse, location, cancellationToken);
        await _forecastRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<ForecastFetch> AddForecastFetchAndLogAsync(
        ForecastApiResponse apiResponse,
        Location location,
        CancellationToken cancellationToken)
    {
        var updatedAt = apiResponse.ForecastResponse?.Properties.Meta.UpdatedAt;

        var forecastFetch = ForecastFetch.Create(
            location,
            "compact",
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

    private sealed record TimeseriesWithNextPeriod(
        MetForecastTimeseries Hourly,
        MetForecastNextPeriod? NextPeriod);

    private sealed record LookupPreparation(
        Dictionary<string, Unit> UnitByValue,
        Dictionary<string, Metric> MetricByName,
        Dictionary<string, WeatherSymbol> WeatherSymbolByCode,
        List<TimeseriesWithNextPeriod> TimeseriesWithNextPeriod);
}
