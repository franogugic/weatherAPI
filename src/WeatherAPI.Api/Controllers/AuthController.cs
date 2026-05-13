using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WeatherAPI.Application.Configuration;
using WeatherAPI.Application.Dtos;
using WeatherAPI.Application.Interfaces;

namespace WeatherAPI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly AuthOptions _authOptions;
    
    public AuthController(IAuthService authService, IOptions<AuthOptions> authOptions)
    {
        _authService = authService;
        _authOptions = authOptions.Value;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var response = await _authService.RegisterAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserRequestDto request, CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(request, cancellationToken);
        Response.Cookies.Append(
            _authOptions.SessionCookieName,
            response.SessionToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = _authOptions.CookieSecure,
                SameSite = SameSiteMode.Lax,
                Expires = response.ExpiresAt
            });

        return Ok(response.User);
    }
    
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var sessionToken = Request.Cookies[_authOptions.SessionCookieName];
        
        if (string.IsNullOrWhiteSpace(sessionToken))
            throw new UnauthorizedAccessException("User is not authenticated");
        
        var response = await _authService.GetCurrentUserAsync(sessionToken, cancellationToken);
        return Ok(response);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var sessionToken = Request.Cookies[_authOptions.SessionCookieName];
        await _authService.LogoutAsync(sessionToken, cancellationToken);
        
        Response.Cookies.Delete(
            _authOptions.SessionCookieName,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = _authOptions.CookieSecure,
                SameSite = SameSiteMode.Lax
            });

        return NoContent();
    }
}
