using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Application.Interfaces;

public interface IUserSessionRepository
{
    Task AddAsync(UserSession userSession, CancellationToken cancellationToken);
    Task<UserSession?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

}