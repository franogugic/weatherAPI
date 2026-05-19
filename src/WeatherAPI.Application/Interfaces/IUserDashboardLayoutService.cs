using WeatherAPI.Application.Dtos;

namespace WeatherAPI.Application.Interfaces;

public interface IUserDashboardLayoutService
{
    Task<GetUserDashboardLayoutResponseDto> GetCurrentUserDashboardLayoutAsync(
        string sessionToken,
        CancellationToken cancellationToken = default);

    Task<GetUserDashboardLayoutResponseDto> UpdateCurrentUserDashboardLayoutAsync(
        string sessionToken,
        UpdateUserDashboardLayoutRequestDto request,
        CancellationToken cancellationToken = default);
}
