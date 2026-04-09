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
        // svi metric-unit iz requesta
        var units = forecastResponse.Properties.Meta.Units;
        // za jedan timeslot se mapira data s next period podacima
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
        // izbaci duplikate iz requesta i ubaci ih u listu
        var distinctUnitValues = unitValues.Distinct().ToList();
        //dohvat svih postojecih unita iz baze, spremanje u dict
        var unitsByValue = (await _forecastRepository.GetUnitsByValuesAsync(distinctUnitValues, cancellationToken))
            .ToDictionary(unit => unit.Value);

        // usporedba req unita s bazom, ako nema kreira novi
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

    // logika ista kao za unit, ako ne postoji u bazi kreira novi
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

    // kao kod unit i metirc
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

    // kreiramo timeseries + next_period 
    private static List<TimeseriesWithNextPeriod> CreateTimeseriesWithNextPeriod(MetForecastResponse forecastResponse)
    {
        return forecastResponse.Properties.Timeseries
            .Select(hourly => new TimeseriesWithNextPeriod(hourly, SelectNextPeriod(hourly)))
            .ToList();
    }

    // odabiremo najblizi next period
    private static MetForecastNextPeriod? SelectNextPeriod(MetForecastTimeseries hourly)
    {
        return hourly.Data.Next1Hours
            ?? hourly.Data.Next6Hours
            ?? hourly.Data.Next12Hours;
    }
}
