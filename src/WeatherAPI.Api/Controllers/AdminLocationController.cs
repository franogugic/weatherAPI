using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WeatherAPI.Application.Common;
using WeatherAPI.Application.Configuration;
using WeatherAPI.Application.Dtos;
using WeatherAPI.Application.Interfaces;

namespace WeatherAPI.Api.Controllers;

[ApiController]
[Route("api/admin/locations")]
public class AdminLocationController : ControllerBase
{
    private readonly AuthOptions _authOptions;
    private readonly IAdminLocationService _adminLocationService;

    public AdminLocationController(
        IOptions<AuthOptions> authOptions,
        IAdminLocationService adminLocationService)
    {
        _authOptions = authOptions.Value;
        _adminLocationService = adminLocationService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateLocation(
        [FromBody] CreateLocationRequestDto request,
        CancellationToken cancellationToken)
    {
        var response = await _adminLocationService.CreateLocationAsync(
            GetSessionToken(),
            request,
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpDelete("{locationId:int}")]
    public async Task<IActionResult> DeleteLocation(
        int locationId,
        CancellationToken cancellationToken)
    {
        await _adminLocationService.DeleteLocationAsync(
            GetSessionToken(),
            ValidateLocationId(locationId),
            cancellationToken);

        return NoContent();
    }

    [HttpGet("{locationId:int}/fetches")]
    public async Task<IActionResult> GetLocationFetches(
        int locationId,
        CancellationToken cancellationToken)
    {
        var response = await _adminLocationService.GetLocationFetchesAsync(
            GetSessionToken(),
            ValidateLocationId(locationId),
            cancellationToken);

        return Ok(response);
    }

    [HttpDelete("{locationId:int}/fetches/{fetchId:int}")]
    public async Task<IActionResult> DeleteLocationFetch(
        int locationId,
        int fetchId,
        CancellationToken cancellationToken)
    {
        if (fetchId <= 0)
        {
            throw new BadRequestException("Fetch id is invalid.");
        }

        await _adminLocationService.DeleteLocationFetchAsync(
            GetSessionToken(),
            ValidateLocationId(locationId),
            fetchId,
            cancellationToken);

        return NoContent();
    }

    private string GetSessionToken()
    {
        var sessionToken = Request.Cookies[_authOptions.SessionCookieName];

        if (string.IsNullOrWhiteSpace(sessionToken))
        {
            throw new UnauthorizedAccessException("User is not authenticated");
        }

        return sessionToken;
    }

    private static short ValidateLocationId(int locationId)
    {
        if (locationId <= 0 || locationId > short.MaxValue)
        {
            throw new BadRequestException("Location id is invalid.");
        }

        return (short)locationId;
    }
}
