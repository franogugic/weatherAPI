using Microsoft.EntityFrameworkCore;
using WeatherAPI.Application.Interfaces;
using WeatherAPI.Domain.Entities;
using WeatherAPI.Infrastructure.Persistence;

namespace WeatherAPI.Infrastructure.Repositories;

public class UserPreferenceRepository : IUserPreferenceRepository
{
    private readonly WeatherDbContext _context;

    public UserPreferenceRepository(WeatherDbContext context)
    {
        _context = context;   
    }
    
    public async Task<UserPreference?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.UserPreferences
            .FirstOrDefaultAsync(pref => pref.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(UserPreference userPreference, CancellationToken cancellationToken = default)
    {
        await _context.UserPreferences.AddAsync(userPreference, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
