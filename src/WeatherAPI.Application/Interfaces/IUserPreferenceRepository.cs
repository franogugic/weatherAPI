using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Application.Interfaces;

public interface IUserPreferenceRepository
{
    Task<UserPreference?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task AddAsync(UserPreference userPreference, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
