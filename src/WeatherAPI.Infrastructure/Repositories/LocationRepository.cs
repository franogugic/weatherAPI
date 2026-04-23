using Microsoft.EntityFrameworkCore;
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

    public async Task<List<Location?>> GetLocationsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Locations.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Location location, CancellationToken cancellationToken = default)
    {
        await _context.Locations.AddAsync(location, cancellationToken);  
    }
    
}
