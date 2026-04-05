namespace WeatherAPI.Application.Models;

public class ForecastApiResponse
{
    public short StatusCode { get; set; }
    public MetForecastResponse? ForecastResponse { get; set; }
    public string? ErrorMessage { get; set; }
}
