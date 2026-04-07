namespace WeatherAPI.Application.Enums;

public enum WeatherForecastFetchStatus
{
    Persisted = 1,
    SkippedUnchanged = 2,
    LoggedWithoutPayload = 3
}
