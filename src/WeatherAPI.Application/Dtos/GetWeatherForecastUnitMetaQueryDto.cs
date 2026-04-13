namespace WeatherAPI.Application.Dtos;

public class GetWeatherForecastUnitMetaQueryDto
{
    public string MetricName { get; set; } = string.Empty;
    public string UnitDisplayName { get; set; } = string.Empty;
    public string? UnitDescription { get; set; }
}
