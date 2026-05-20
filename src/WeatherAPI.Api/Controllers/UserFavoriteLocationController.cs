using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WeatherAPI.Api.Extensions;
using WeatherAPI.Application.Configuration;
using WeatherAPI.Application.Dtos;
using WeatherAPI.Application.Interfaces;

namespace WeatherAPI.Api.Controllers;

[ApiController]
[Route("api/user-favorite-locations")]
public class UserFavoriteLocationController : ControllerBase
{
    private readonly AuthOptions _authOptions;
    private readonly IUserFavoriteLocationService _userFavoriteLocationService;

    public UserFavoriteLocationController(
        IOptions<AuthOptions> authOptions,
        IUserFavoriteLocationService userFavoriteLocationService)
    {
        _authOptions = authOptions.Value;
        _userFavoriteLocationService = userFavoriteLocationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetFavoriteLocations(CancellationToken cancellationToken)
    {
        var sessionToken = GetSessionToken();
        var response = await _userFavoriteLocationService.GetCurrentUserFavoriteLocationsAsync(
            sessionToken,
            cancellationToken);

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> AddFavoriteLocation(
        [FromBody] AddUserFavoriteLocationRequestDto request,
        CancellationToken cancellationToken)
    {
        var sessionToken = GetSessionToken();
        var response = await _userFavoriteLocationService.AddCurrentUserFavoriteLocationAsync(
            sessionToken,
            request.LocationId,
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpDelete("{locationId}")]
    public async Task<IActionResult> RemoveFavoriteLocation(
        short locationId,
        CancellationToken cancellationToken)
    {
        var sessionToken = GetSessionToken();
        await _userFavoriteLocationService.RemoveCurrentUserFavoriteLocationAsync(
            sessionToken,
            locationId,
            cancellationToken);

        return NoContent();
    }

    private string GetSessionToken()
    {
        return Request.GetSessionToken(_authOptions);
    }
}
