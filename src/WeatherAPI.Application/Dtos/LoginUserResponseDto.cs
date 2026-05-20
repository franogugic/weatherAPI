using WeatherAPI.Domain.Enums;

namespace WeatherAPI.Application.Dtos;

public class LoginUserResponseDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string SessionToken { get; set; } = string.Empty;
}
