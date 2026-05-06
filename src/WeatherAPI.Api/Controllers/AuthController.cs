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
}
