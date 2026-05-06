using WeatherAPI.Application.Interfaces;
using WeatherAPI.Domain.Entities;
using WeatherAPI.Infrastructure.Persistence;

namespace WeatherAPI.Infrastructure.Repositories;

public class UserSessionRepository : IUserSessionRepository
{
    private readonly WeatherDbContext _context;
    
    public UserSessionRepository(WeatherDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(UserSession userSession, CancellationToken cancellationToken = default)
    {
        await _context.UserSessions.AddAsync(userSession, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
