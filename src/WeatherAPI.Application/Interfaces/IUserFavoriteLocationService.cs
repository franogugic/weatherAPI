using WeatherAPI.Application.Dtos;

namespace WeatherAPI.Application.Interfaces;

public interface IUserFavoriteLocationService
{
    Task<List<GetLocationResponseDto>> GetCurrentUserFavoriteLocationsAsync(
        string sessionToken,
        CancellationToken cancellationToken = default);

    Task<GetLocationResponseDto> AddCurrentUserFavoriteLocationAsync(
        string sessionToken,
        short locationId,
        CancellationToken cancellationToken = default);

    Task RemoveCurrentUserFavoriteLocationAsync(
        string sessionToken,
        short locationId,
        CancellationToken cancellationToken = default);
}
