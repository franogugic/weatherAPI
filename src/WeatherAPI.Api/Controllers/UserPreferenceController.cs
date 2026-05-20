using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WeatherAPI.Api.Extensions;
using WeatherAPI.Application.Configuration;
using WeatherAPI.Application.Dtos;
using WeatherAPI.Application.Interfaces;

namespace WeatherAPI.Api.Controllers;

[ApiController]
[Route("api/user-preferences")]
public class UserPreferenceController : ControllerBase
{
    private readonly AuthOptions _authOptions;
    private readonly IUserPreferenceService _userPreferenceService;

    public UserPreferenceController(IOptions<AuthOptions> authOptions, IUserPreferenceService userPreferenceService)
    {
        _authOptions = authOptions.Value;
        _userPreferenceService = userPreferenceService;
    }
        
        
    [HttpGet]
    public async Task<IActionResult> GetUserPreference(CancellationToken cancellationToken)
    {
        var sessionToken = GetSessionToken();
        var response = await _userPreferenceService.GetCurrentUserPreferencesAsync(sessionToken, cancellationToken);
        return Ok(response);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateUserPreference([FromBody] UpdateUserPreferenceRequestDto request,CancellationToken cancellationToken)
    {
        var sessionToken = GetSessionToken();
        var response = await _userPreferenceService.UpdateCurrentUserPreferencesAsync(sessionToken, request, cancellationToken);
        return Ok(response); 
    }

    private string GetSessionToken()
    {
        return Request.GetSessionToken(_authOptions);
    }
}
