namespace WeatherAPI.Application.Dtos;

// dict - value dio, uniti za svaki fetch
public class GetWeatherForecastUnitMetaDto
{
    public string UnitDisplayName { get; set; } = string.Empty;
    public string? UnitDescription { get; set; }
}
