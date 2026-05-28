namespace WeatherAPI.Application.Dtos;

public class ChatWeatherForecastContextDto
{
    public short LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public short? Altitude { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime FetchedAt { get; set; }
    public ChatWeatherForecastItemDto? Current { get; set; }
    public List<ChatWeatherForecastItemDto> Upcoming { get; set; } = [];
}
