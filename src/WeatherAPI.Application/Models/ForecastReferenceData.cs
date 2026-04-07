using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Application.Models;

public sealed record ForecastReferenceData(
    Dictionary<string, Unit> UnitByValue,
    Dictionary<string, Metric> MetricByName,
    Dictionary<string, WeatherSymbol> WeatherSymbolByCode,
    List<TimeseriesWithNextPeriod> TimeseriesWithNextPeriod);
