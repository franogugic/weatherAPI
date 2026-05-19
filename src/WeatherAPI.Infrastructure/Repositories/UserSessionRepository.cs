using Microsoft.EntityFrameworkCore;
using WeatherAPI.Application.Interfaces;
using WeatherAPI.Domain.Entities;
using WeatherAPI.Infrastructure.Persistence;

namespace WeatherAPI.Infrastructure.Repositories;

public class UserSessionRepository : IUserSessionRepository
{
    private readonly UserDbContext _context;
    
    public UserSessionRepository(UserDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(UserSession userSession, CancellationToken cancellationToken = default)
    {
        await _context.UserSessions.AddAsync(userSession, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserSession?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.UserSessions
            .AsNoTracking()
            .Include(session => session.User)
            .FirstOrDefaultAsync(session => session.TokenHash == token, cancellationToken);
    }

    public async Task RevokeByTokenAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        var session = await _context.UserSessions
            .FirstOrDefaultAsync(session => session.TokenHash == tokenHash, cancellationToken);

        if (session is null || session.RevokedAt is not null)
            return;

        session.Revoke();
        await _context.SaveChangesAsync(cancellationToken);
    }
}
