using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WeatherAPI.Application.Configuration;
using WeatherAPI.Application.Dtos;
using WeatherAPI.Application.Interfaces;

namespace WeatherAPI.Api.Controllers;

[ApiController]
[Route("api/user-dashboard-layout")]
public class UserDashboardLayoutController : ControllerBase
{
    private readonly AuthOptions _authOptions;
    private readonly IUserDashboardLayoutService _dashboardLayoutService;

    public UserDashboardLayoutController(
        IOptions<AuthOptions> authOptions,
        IUserDashboardLayoutService dashboardLayoutService)
    {
        _authOptions = authOptions.Value;
        _dashboardLayoutService = dashboardLayoutService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboardLayout(CancellationToken cancellationToken)
    {
        var response = await _dashboardLayoutService.GetCurrentUserDashboardLayoutAsync(
            GetSessionToken(),
            cancellationToken);

        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateDashboardLayout(
        [FromBody] UpdateUserDashboardLayoutRequestDto request,
        CancellationToken cancellationToken)
    {
        var response = await _dashboardLayoutService.UpdateCurrentUserDashboardLayoutAsync(
            GetSessionToken(),
            request,
            cancellationToken);

        return Ok(response);
    }

    private string GetSessionToken()
    {
        var sessionToken = Request.Cookies[_authOptions.SessionCookieName];

        if (string.IsNullOrWhiteSpace(sessionToken))
            throw new UnauthorizedAccessException("User is not authenticated");

        return sessionToken;
    }
}
