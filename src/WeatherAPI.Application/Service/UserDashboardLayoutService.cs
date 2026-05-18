using System.Text.Json;
using WeatherAPI.Application.Common;
using WeatherAPI.Application.Dtos;
using WeatherAPI.Application.Interfaces;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Application.Service;

public class UserDashboardLayoutService : IUserDashboardLayoutService
{
    private readonly IAuthService _authService;
    private readonly IUserDashboardLayoutRepository _dashboardLayoutRepository;

    public UserDashboardLayoutService(
        IAuthService authService,
        IUserDashboardLayoutRepository dashboardLayoutRepository)
    {
        _authService = authService;
        _dashboardLayoutRepository = dashboardLayoutRepository;
    }

    public async Task<GetUserDashboardLayoutResponseDto> GetCurrentUserDashboardLayoutAsync(
        string sessionToken,
        CancellationToken cancellationToken = default)
    {
        var user = await _authService.GetCurrentUserAsync(sessionToken, cancellationToken);
        var dashboardLayout = await _dashboardLayoutRepository.GetByUserIdAsync(user.Id, cancellationToken);

        return new GetUserDashboardLayoutResponseDto
        {
            LayoutJson = dashboardLayout?.LayoutJson ?? string.Empty
        };
    }

    public async Task<GetUserDashboardLayoutResponseDto> UpdateCurrentUserDashboardLayoutAsync(
        string sessionToken,
        UpdateUserDashboardLayoutRequestDto request,
        CancellationToken cancellationToken = default)
    {
        if (!IsValidJson(request.LayoutJson))
        {
            throw new BadRequestException("Dashboard layout must be valid JSON.");
        }

        var user = await _authService.GetCurrentUserAsync(sessionToken, cancellationToken);
        var dashboardLayout = await _dashboardLayoutRepository.GetByUserIdAsync(user.Id, cancellationToken);

        if (dashboardLayout is null)
        {
            dashboardLayout = UserDashboardLayout.Create(user.Id, request.LayoutJson);
            await _dashboardLayoutRepository.AddAsync(dashboardLayout, cancellationToken);
        }
        else
        {
            dashboardLayout.Update(request.LayoutJson);
            await _dashboardLayoutRepository.SaveChangesAsync(cancellationToken);
        }

        return new GetUserDashboardLayoutResponseDto
        {
            LayoutJson = dashboardLayout.LayoutJson
        };
    }

    private static bool IsValidJson(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        try
        {
            JsonDocument.Parse(value);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
