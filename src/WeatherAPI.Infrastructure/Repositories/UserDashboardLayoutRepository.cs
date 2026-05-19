using Microsoft.EntityFrameworkCore;
using WeatherAPI.Application.Interfaces;
using WeatherAPI.Domain.Entities;
using WeatherAPI.Infrastructure.Persistence;

namespace WeatherAPI.Infrastructure.Repositories;

public class UserDashboardLayoutRepository : IUserDashboardLayoutRepository
{
    private readonly UserDbContext _context;

    public UserDashboardLayoutRepository(UserDbContext context)
    {
        _context = context;
    }

    public async Task<UserDashboardLayout?> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserDashboardLayouts
            .FirstOrDefaultAsync(layout => layout.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(
        UserDashboardLayout dashboardLayout,
        CancellationToken cancellationToken = default)
    {
        await _context.UserDashboardLayouts.AddAsync(dashboardLayout, cancellationToken);
        await SaveChangesAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
