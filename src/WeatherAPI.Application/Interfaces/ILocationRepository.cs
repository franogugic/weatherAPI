using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Application.Interfaces;

public interface ILocationRepository
{
    Task<Location?> GetLocationAsync(decimal latitude, decimal longitude, short? altitude,
        CancellationToken cancellationToken = default);
    Task<List<Location>> GetLocationsAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Location location, CancellationToken cancellationToken = default);
}
