namespace WeatherAPI.Application.Dtos;

public class LoginUserResultDto
{
    public LoginUserResponseDto User { get; set; } = new();
    public string SessionToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
