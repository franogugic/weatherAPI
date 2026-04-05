namespace WeatherAPI.Domain.Entities;

public class WeatherSymbol
{
    private WeatherSymbol()
    {
    }

    private WeatherSymbol(string code)
    {
        Code = code;
    }

    public byte Id { get; private set; }
    public string Code { get; private set; } = string.Empty;

    public ICollection<HourlyForecast> HourlyForecasts { get; private set; } = new List<HourlyForecast>();

    public static WeatherSymbol Create(string code)
    {
        return new WeatherSymbol(code);
    }
}
