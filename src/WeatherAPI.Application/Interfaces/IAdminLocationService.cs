using WeatherAPI.Application.Dtos;

namespace WeatherAPI.Application.Interfaces;

public interface IAdminLocationService
{
    Task<GetLocationResponseDto> CreateLocationAsync(
        string sessionToken,
        CreateLocationRequestDto request,
        CancellationToken cancellationToken = default);

    Task DeleteLocationAsync(
        string sessionToken,
        short locationId,
        CancellationToken cancellationToken = default);
}
