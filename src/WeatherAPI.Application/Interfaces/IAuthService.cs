using WeatherAPI.Application.Dtos;

namespace WeatherAPI.Application.Interfaces;

public interface IAuthService
{
    Task<RegisterUserResponseDto> RegisterAsync(RegisterUserRequestDto request, CancellationToken cancellationToken);
    Task<LoginUserResultDto> LoginAsync(LoginUserRequestDto request, CancellationToken cancellationToken);

    Task<CurrentUserResponseDto> GetCurrentUserAsync(string sessionToken, CancellationToken cancellationToken);
}
