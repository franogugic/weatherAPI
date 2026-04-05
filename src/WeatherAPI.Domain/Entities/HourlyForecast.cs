namespace WeatherAPI.Domain.Entities;

public class HourlyForecast
{
    private HourlyForecast()
    {
    }

    private HourlyForecast(
        ForecastFetch forecastFetch,
        DateTime forecastTime,
        decimal? airTemperature,
        decimal? airPressureAtSeaLevel,
        byte? cloudiness,
        byte? humidity,
        decimal? windDirection,
        decimal? windSpeed,
        WeatherSymbol? weatherSymbol,
        decimal? precipitationAmount)
    {
        ForecastFetch = forecastFetch;
        ForecastTime = forecastTime;
        AirTemperature = airTemperature;
        AirPressureAtSeaLevel = airPressureAtSeaLevel;
        Cloudiness = cloudiness;
        Humidity = humidity;
        WindDirection = windDirection;
        WindSpeed = windSpeed;
        WeatherSymbol = weatherSymbol;
        PrecipitationAmount = precipitationAmount;
    }

    public int ForecastFetchId { get; private set; }
    public DateTime ForecastTime { get; private set; }
    public decimal? AirTemperature { get; private set; }
    public decimal? AirPressureAtSeaLevel { get; private set; }
    public byte? Cloudiness { get; private set; }
    public byte? Humidity { get; private set; }
    public decimal? WindDirection { get; private set; }
    public decimal? WindSpeed { get; private set; }
    public byte? WeatherSymbolId { get; private set; }
    public decimal? PrecipitationAmount { get; private set; }

    public ForecastFetch? ForecastFetch { get; private set; }
    public WeatherSymbol? WeatherSymbol { get; private set; }

    public static HourlyForecast Create(
        ForecastFetch forecastFetch,
        DateTime forecastTime,
        decimal? airTemperature,
        decimal? airPressureAtSeaLevel,
        byte? cloudiness,
        byte? humidity,
        decimal? windDirection,
        decimal? windSpeed,
        WeatherSymbol? weatherSymbol,
        decimal? precipitationAmount)
    {
        return new HourlyForecast(
            forecastFetch,
            forecastTime,
            airTemperature,
            airPressureAtSeaLevel,
            cloudiness,
            humidity,
            windDirection,
            windSpeed,
            weatherSymbol,
            precipitationAmount);
    }
}
