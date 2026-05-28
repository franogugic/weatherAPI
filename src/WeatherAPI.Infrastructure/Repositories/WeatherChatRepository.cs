using Microsoft.EntityFrameworkCore;
using WeatherAPI.Application.Dtos;
using WeatherAPI.Application.Interfaces;
using WeatherAPI.Infrastructure.Persistence;

namespace WeatherAPI.Infrastructure.Repositories;

public class WeatherChatRepository : IWeatherChatRepository
{
    private readonly WeatherDbContext _context;

    public WeatherChatRepository(WeatherDbContext context)
    {
        _context = context;
    }

    public async Task<ChatWeatherForecastContextDto?> GetForecastContextAsync(
        short locationId,
        int days,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var start = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0, DateTimeKind.Utc);
        var end = start.AddDays(days);

        var fetch = await _context.ForecastFetches
            .AsNoTracking()
            .Where(forecastFetch => forecastFetch.LocationId == locationId
                                    && forecastFetch.FetchLog != null
                                    && forecastFetch.FetchLog.StatusCode == 200
                                    && forecastFetch.HourlyForecasts.Any())
            .OrderByDescending(forecastFetch => forecastFetch.FetchedAt)
            .Select(forecastFetch => new
            {
                forecastFetch.LocationId,
                LocationName = forecastFetch.Location != null && forecastFetch.Location.Name != null
                    ? forecastFetch.Location.Name
                    : $"Location {forecastFetch.LocationId}",
                Latitude = forecastFetch.Location != null ? forecastFetch.Location.Latitude : 0,
                Longitude = forecastFetch.Location != null ? forecastFetch.Location.Longitude : 0,
                Altitude = forecastFetch.Location != null ? forecastFetch.Location.Altitude : null,
                forecastFetch.UpdatedAt,
                forecastFetch.FetchedAt,
                Current = forecastFetch.HourlyForecasts
                    .Where(hourly => hourly.ForecastTime <= now)
                    .OrderByDescending(hourly => hourly.ForecastTime)
                    .Select(hourly => new ChatWeatherForecastItemDto
                    {
                        ForecastTime = DateTime.SpecifyKind(hourly.ForecastTime, DateTimeKind.Utc),
                        AirTemperature = hourly.AirTemperature,
                        AirPressureAtSeaLevel = hourly.AirPressureAtSeaLevel,
                        Cloudiness = hourly.Cloudiness,
                        Humidity = hourly.Humidity,
                        WindDirection = hourly.WindDirection,
                        WindSpeed = hourly.WindSpeed,
                        WeatherSymbol = hourly.WeatherSymbol != null ? hourly.WeatherSymbol.Code : null,
                        PrecipitationAmount = hourly.PrecipitationAmount
                    })
                    .FirstOrDefault(),
                Upcoming = forecastFetch.HourlyForecasts
                    .Where(hourly => hourly.ForecastTime >= start
                                     && hourly.ForecastTime < end)
                    .OrderBy(hourly => hourly.ForecastTime)
                    .Select(hourly => new ChatWeatherForecastItemDto
                    {
                        ForecastTime = DateTime.SpecifyKind(hourly.ForecastTime, DateTimeKind.Utc),
                        AirTemperature = hourly.AirTemperature,
                        AirPressureAtSeaLevel = hourly.AirPressureAtSeaLevel,
                        Cloudiness = hourly.Cloudiness,
                        Humidity = hourly.Humidity,
                        WindDirection = hourly.WindDirection,
                        WindSpeed = hourly.WindSpeed,
                        WeatherSymbol = hourly.WeatherSymbol != null ? hourly.WeatherSymbol.Code : null,
                        PrecipitationAmount = hourly.PrecipitationAmount
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (fetch is null)
            return null;

        return new ChatWeatherForecastContextDto
        {
            LocationId = fetch.LocationId,
            LocationName = fetch.LocationName,
            Latitude = fetch.Latitude,
            Longitude = fetch.Longitude,
            Altitude = fetch.Altitude,
            UpdatedAt = fetch.UpdatedAt,
            FetchedAt = DateTime.SpecifyKind(fetch.FetchedAt, DateTimeKind.Utc),
            Current = fetch.Current,
            Upcoming = fetch.Upcoming
        };
    }
}
