namespace WeatherAPI.Application.Dtos;

public class GetLocationCurrentWeatherDto
{
    public decimal? AirTemperature { get; set; }
    public string? WeatherSymbol { get; set; }
}
