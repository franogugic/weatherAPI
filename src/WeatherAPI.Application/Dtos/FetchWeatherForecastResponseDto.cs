using WeatherAPI.Application.Enums;

namespace WeatherAPI.Application.Dtos;

public class FetchWeatherForecastResponseDto
{
    public WeatherForecastFetchStatus Status { get; set; }
    public int HourlyForecastCount { get; set; }

    public string Message => Status switch
    {
        WeatherForecastFetchStatus.Persisted => "Forecast data fetched and persisted successfully.",
        WeatherForecastFetchStatus.SkippedUnchanged => "Forecast fetch was logged, but forecast data was unchanged and was not persisted again.",
        WeatherForecastFetchStatus.LoggedWithoutPayload => "Forecast fetch attempt was logged, but no forecast payload was returned.",
        _ => "Forecast fetch completed."
    };
}
