namespace WeatherAPI.Application.Dtos;

public class GetLocationResponseDto
{
    public short Id { get; set; }
    public string? Name { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public short? Altitude { get; set; }
    public GetLocationCurrentWeatherDto? CurrentWeather { get; set; }
}
