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
        var isHttpsRequest = Request.IsHttps ||
                             string.Equals(
                                 Request.Headers["X-Forwarded-Proto"],
                                 "https",
                                 StringComparison.OrdinalIgnoreCase);
        var isCrossOriginRequest = IsCrossOriginRequest();

        if (isHttpsRequest && isCrossOriginRequest)
        {
            sameSiteMode = SameSiteMode.None;
        }

        return new CookieOptions
        {
            HttpOnly = true,
            Secure = _authOptions.CookieSecure || isHttpsRequest,
            SameSite = sameSiteMode,
            Path = "/",
            Expires = expiresAt.HasValue
                ? new DateTimeOffset(DateTime.SpecifyKind(expiresAt.Value, DateTimeKind.Utc))
                : null
        };
    }

    private bool IsCrossOriginRequest()
    {
        var origin = Request.Headers.Origin.ToString();

        if (string.IsNullOrWhiteSpace(origin))
        {
            return false;
        }

        var requestOrigin = $"{Request.Scheme}://{Request.Host}";
        return !string.Equals(origin, requestOrigin, StringComparison.OrdinalIgnoreCase);
    }
}
