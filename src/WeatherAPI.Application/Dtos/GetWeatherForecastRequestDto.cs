namespace WeatherAPI.Application.Dtos;

public class GetWeatherForecastRequestDto
{
    public int? Days { get; set; } = 3;
    
    public short LocationId { get; set; }
}
