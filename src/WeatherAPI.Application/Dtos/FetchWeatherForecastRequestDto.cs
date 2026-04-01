namespace WeatherAPI.Application.Dtos;

public class FetchWeatherForecastRequestDto
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public short? Altitude { get; set; }
}
