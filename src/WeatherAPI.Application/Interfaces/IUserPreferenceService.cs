using WeatherAPI.Application.Dtos;

namespace WeatherAPI.Application.Interfaces;


public interface IUserPreferenceService
{
    Task<GetUserPreferenceResponseDto> GetCurrentUserPreferencesAsync(
        string sessionToken,
        CancellationToken cancellationToken = default);
}
