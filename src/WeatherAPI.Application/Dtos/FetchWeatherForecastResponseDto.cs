namespace WeatherAPI.Application.Dtos;

public class FetchWeatherForecastResponseDto
{
    public string Message { get; set; } = string.Empty;
    public bool ForecastDataPersisted { get; set; }
    public bool SkippedBecauseForecastUnchanged { get; set; }
    public int HourlyForecastCount { get; set; }
}
