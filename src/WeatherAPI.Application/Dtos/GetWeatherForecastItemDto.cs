namespace WeatherAPI.Application.Dtos;

// jedan hourly zapis
public class GetWeatherForecastItemDto
{
    public DateTime ForecastTime { get; set; }
    public decimal? AirTemperature { get; set; }
    public decimal? AirPressureAtSeaLevel { get; set; }
    public byte? Cloudiness { get; set; }
    public byte? Humidity { get; set; }
    public decimal? WindDirection { get; set; }
    public decimal? WindSpeed { get; set; }
    public string? WeatherSymbol { get; set; }
    public decimal? PrecipitationAmount { get; set; }
}
