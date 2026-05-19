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
            BuildSessionCookieOptions(response.ExpiresAt));

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
            BuildSessionCookieOptions());

        return NoContent();
    }

    private SameSiteMode GetCookieSameSiteMode()
    {
        return Enum.TryParse<SameSiteMode>(
            _authOptions.CookieSameSite,
            ignoreCase: true,
            out var sameSiteMode)
            ? sameSiteMode
            : SameSiteMode.Lax;
    }

    private CookieOptions BuildSessionCookieOptions(DateTime? expiresAt = null)
    {
        var sameSiteMode = GetCookieSameSiteMode();
        var forwardedProtoHeader = Request.Headers["X-Forwarded-Proto"].ToString();
        var isHttpsRequest = Request.IsHttps || forwardedProtoHeader
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(protocol => string.Equals(protocol, "https", StringComparison.OrdinalIgnoreCase));
        var hasOriginHeader = !string.IsNullOrWhiteSpace(Request.Headers.Origin.ToString());

        if (isHttpsRequest && hasOriginHeader)
        {
            sameSiteMode = SameSiteMode.None;
        }

        var expiresAtUtc = expiresAt?.ToUniversalTime();

        return new CookieOptions
        {
            HttpOnly = true,
            Secure = _authOptions.CookieSecure || isHttpsRequest,
            SameSite = sameSiteMode,
            Path = "/",
            Expires = expiresAtUtc.HasValue
                ? new DateTimeOffset(DateTime.SpecifyKind(expiresAtUtc.Value, DateTimeKind.Utc))
                : null,
            MaxAge = expiresAtUtc.HasValue
                ? expiresAtUtc.Value - DateTime.UtcNow
                : null
        };
    }
}
