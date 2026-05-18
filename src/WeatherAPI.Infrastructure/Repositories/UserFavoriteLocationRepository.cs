using Microsoft.EntityFrameworkCore;
using WeatherAPI.Application.Interfaces;
using WeatherAPI.Domain.Entities;
using WeatherAPI.Infrastructure.Persistence;

namespace WeatherAPI.Infrastructure.Repositories;

public class UserFavoriteLocationRepository : IUserFavoriteLocationRepository
{
    private readonly UserDbContext _context;

    public UserFavoriteLocationRepository(UserDbContext context)
    {
        _context = context;  
    }
    public async Task<List<UserFavoriteLocation>> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserFavoriteLocations
            .AsNoTracking()
            .Where(location => location.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserFavoriteLocation?> GetAsync(
        int userId,
        short locationId,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserFavoriteLocations
            .FirstOrDefaultAsync(
                favorite => favorite.UserId == userId && favorite.LocationId == locationId,
                cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        int userId,
        short locationId,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserFavoriteLocations
            .AnyAsync(
                favorite => favorite.UserId == userId && favorite.LocationId == locationId,
                cancellationToken);
    }

    public async Task AddAsync(
        UserFavoriteLocation favoriteLocation,
        CancellationToken cancellationToken = default)
    {
        await _context.UserFavoriteLocations.AddAsync(favoriteLocation, cancellationToken);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(
        UserFavoriteLocation favoriteLocation,
        CancellationToken cancellationToken = default)
    {
        _context.UserFavoriteLocations.Remove(favoriteLocation);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
