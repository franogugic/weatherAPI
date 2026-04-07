namespace WeatherAPI.Application.Models;

public sealed record TimeseriesWithNextPeriod(
    MetForecastTimeseries Hourly,
    MetForecastNextPeriod? NextPeriod);
