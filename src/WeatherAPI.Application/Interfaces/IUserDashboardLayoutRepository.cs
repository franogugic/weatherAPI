using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Application.Interfaces;

public interface IUserDashboardLayoutRepository
{
    Task<UserDashboardLayout?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task AddAsync(UserDashboardLayout dashboardLayout, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
