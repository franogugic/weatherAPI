namespace WeatherAPI.Application.Dtos;

// GET response
public class GetWeatherForecastResponseDto
{
    public Dictionary<string, GetWeatherForecastUnitMetaDto> Meta { get; set; } = new();
    public List<GetWeatherForecastItemDto> Items { get; set; } = [];
}
