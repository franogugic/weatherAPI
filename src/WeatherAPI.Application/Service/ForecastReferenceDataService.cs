using WeatherAPI.Application.Interfaces;
using WeatherAPI.Application.Models;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Application.Service;

public class ForecastReferenceDataService : IForecastReferenceDataService
{
    private readonly IForecastRepository _forecastRepository;

    public ForecastReferenceDataService(IForecastRepository forecastRepository)
    {
        _forecastRepository = forecastRepository;
    }

    public async Task<ForecastReferenceData> PrepareReferenceDataAsync(
        MetForecastResponse forecastResponse,
        CancellationToken cancellationToken = default)
    {
        var units = forecastResponse.Properties.Meta.Units;
        var timeseriesWithNextPeriod = CreateTimeseriesWithNextPeriod(forecastResponse);

        var unitsByValue = await GetOrCreateUnitsAsync(units.Values, cancellationToken);
        var metricByName = await GetOrCreateMetricsAsync(units.Keys, cancellationToken);
        var weatherSymbolByCode = await GetOrCreateWeatherSymbolsAsync(
            timeseriesWithNextPeriod,
            cancellationToken);

        return new ForecastReferenceData(
            unitsByValue,
            metricByName,
            weatherSymbolByCode,
            timeseriesWithNextPeriod);
    }

    private async Task<Dictionary<string, Unit>> GetOrCreateUnitsAsync(
        IEnumerable<string> unitValues,
        CancellationToken cancellationToken)
    {
        var distinctUnitValues = unitValues.Distinct().ToList();
        var unitsByValue = (await _forecastRepository.GetUnitsByValuesAsync(distinctUnitValues, cancellationToken))
            .ToDictionary(unit => unit.Value);

        foreach (var unitValue in distinctUnitValues)
        {
            if (unitsByValue.ContainsKey(unitValue))
            {
                continue;
            }

            var unit = Unit.Create(unitValue, unitValue, null);
            await _forecastRepository.AddUnitAsync(unit, cancellationToken);
            unitsByValue[unitValue] = unit;
        }

        return unitsByValue;
    }

    private async Task<Dictionary<string, Metric>> GetOrCreateMetricsAsync(
        IEnumerable<string> metricNames,
        CancellationToken cancellationToken)
    {
        var distinctMetricNames = metricNames.Distinct().ToList();
        var metricByName = (await _forecastRepository.GetMetricsByNamesAsync(distinctMetricNames, cancellationToken))
            .ToDictionary(metric => metric.Name);

        foreach (var metricName in distinctMetricNames)
        {
            if (metricByName.ContainsKey(metricName))
            {
                continue;
            }

            var metric = Metric.Create(metricName);
            await _forecastRepository.AddMetricAsync(metric, cancellationToken);
            metricByName[metricName] = metric;
        }

        return metricByName;
    }

    private async Task<Dictionary<string, WeatherSymbol>> GetOrCreateWeatherSymbolsAsync(
        IEnumerable<TimeseriesWithNextPeriod> timeseriesWithNextPeriod,
        CancellationToken cancellationToken)
    {
        var symbolCodes = timeseriesWithNextPeriod
            .Select(entry => entry.NextPeriod?.Summary?.SymbolCode)
            .Where(symbolCode => !string.IsNullOrWhiteSpace(symbolCode))
            .Cast<string>()
            .Distinct()
            .ToList();

        var weatherSymbolByCode = (await _forecastRepository.GetWeatherSymbolsByCodesAsync(symbolCodes, cancellationToken))
            .ToDictionary(symbol => symbol.Code);

        foreach (var symbolCode in symbolCodes)
        {
            if (weatherSymbolByCode.ContainsKey(symbolCode))
            {
                continue;
            }

            var weatherSymbol = WeatherSymbol.Create(symbolCode);
            await _forecastRepository.AddWeatherSymbolAsync(weatherSymbol, cancellationToken);
            weatherSymbolByCode[symbolCode] = weatherSymbol;
        }

        return weatherSymbolByCode;
    }

    private static List<TimeseriesWithNextPeriod> CreateTimeseriesWithNextPeriod(MetForecastResponse forecastResponse)
    {
        return forecastResponse.Properties.Timeseries
            .Select(hourly => new TimeseriesWithNextPeriod(hourly, SelectNextPeriod(hourly)))
            .ToList();
    }

    private static MetForecastNextPeriod? SelectNextPeriod(MetForecastTimeseries hourly)
    {
        return hourly.Data.Next1Hours
            ?? hourly.Data.Next6Hours
            ?? hourly.Data.Next12Hours;
    }
}
