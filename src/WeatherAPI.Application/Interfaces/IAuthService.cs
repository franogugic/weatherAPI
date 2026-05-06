using WeatherAPI.Application.Dtos;

namespace WeatherAPI.Application.Interfaces;

public interface IAuthService
{
    Task<RegisterUserResponseDto> RegisterAsync(RegisterUserRequestDto request, CancellationToken cancellationToken);
}