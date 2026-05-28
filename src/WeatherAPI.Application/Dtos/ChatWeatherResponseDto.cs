namespace WeatherAPI.Application.Dtos;

public class ChatWeatherResponseDto
{
    public string Answer { get; set; } = string.Empty;
    public string LocationName { get; set; } = string.Empty;
    public DateTime? DataUpdatedAt { get; set; }
}
