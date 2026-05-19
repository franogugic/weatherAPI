using Microsoft.EntityFrameworkCore;
using WeatherAPI.Application.Dtos;
using WeatherAPI.Application.Interfaces;
using WeatherAPI.Domain.Entities;
using WeatherAPI.Infrastructure.Persistence;

namespace WeatherAPI.Infrastructure.Repositories;

public class LocationRepository : ILocationRepository
{
    private readonly WeatherDbContext _context;
    
    public LocationRepository(WeatherDbContext context)
    {
        _context = context;
    }
    
    public async Task<Location?> GetLocationAsync(decimal latitude, decimal longitude, short? altitude,
        CancellationToken cancellationToken = default)
    {
        return await _context.Locations
            .SingleOrDefaultAsync(
                l => l.Latitude == latitude && l.Longitude == longitude && l.Altitude == altitude, cancellationToken);
    }

    public async Task<Location?> GetByIdAsync(short locationId, CancellationToken cancellationToken = default)
    {
        return await _context.Locations
            .AsNoTracking()
            .FirstOrDefaultAsync(location => location.Id == locationId, cancellationToken);
    }

    public async Task<List<Location>> GetLocationsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Locations
            .OrderBy(location => location.Name)
            .ThenBy(location => location.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<GetLocationResponseDto>> GetLocationsWithCurrentWeatherAsync(CancellationToken cancellationToken = default)
    {
        return await GetLocationsWithCurrentWeatherQuery()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<GetLocationResponseDto>> GetLocationsWithCurrentWeatherAsync(
        IEnumerable<short> locationIds,
        CancellationToken cancellationToken = default)
    {
        var locationIdList = locationIds.ToList();

        if (locationIdList.Count == 0)
            return [];

        return await GetLocationsWithCurrentWeatherQuery()
            .Where(location => locationIdList.Contains(location.Id))
            .ToListAsync(cancellationToken);
    }

    private IQueryable<GetLocationResponseDto> GetLocationsWithCurrentWeatherQuery()
    {
        var now = DateTime.UtcNow;

        return _context.Locations
            .AsNoTracking()
            .Select(location => new GetLocationResponseDto
            {
                Id = location.Id,
                Name = location.Name,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                Altitude = location.Altitude,
                CurrentWeather = _context.ForecastFetches
                    .Where(fetch => fetch.LocationId == location.Id
                                    && fetch.FetchLog != null
                                    && fetch.FetchLog.StatusCode == 200
                                    && fetch.HourlyForecasts.Any())
                    .OrderByDescending(fetch => fetch.FetchedAt)
                    .Take(1)
                    .SelectMany(fetch => fetch.HourlyForecasts
                        .Where(hourly => hourly.ForecastTime <= now)
                        .OrderByDescending(hourly => hourly.ForecastTime)
                        .Take(1)
                        .Select(hourly => new GetLocationCurrentWeatherDto
                        {
                            AirTemperature = hourly.AirTemperature,
                            WeatherSymbol = hourly.WeatherSymbol != null ? hourly.WeatherSymbol.Code : null
                        }))
                    .FirstOrDefault()
            });
    }

    public async Task AddAsync(Location location, CancellationToken cancellationToken = default)
    {
        await _context.Locations.AddAsync(location, cancellationToken);  
    }

    public async Task<bool> DeleteAsync(short locationId, CancellationToken cancellationToken = default)
    {
        var location = await _context.Locations
            .FirstOrDefaultAsync(location => location.Id == locationId, cancellationToken);

        if (location is null)
        {
            return false;
        }

        _context.Locations.Remove(location);
        await SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
