using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Application.Interfaces;

public interface IUserFavoriteLocationRepository
{
    Task<List<UserFavoriteLocation>> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<UserFavoriteLocation?> GetAsync(
        int userId,
        short locationId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        int userId,
        short locationId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        UserFavoriteLocation favoriteLocation,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(
        UserFavoriteLocation favoriteLocation,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
