namespace WeatherAPI.Domain.Entities;

public class FetchLog
{
    private FetchLog()
    {
    }

    private FetchLog(int forecastFetchId, short statusCode, string? errorMessage)
    {
        ForecastFetchId = forecastFetchId;
        StatusCode = statusCode;
        ErrorMessage = errorMessage;
    }

    public int Id { get; private set; }
    public int ForecastFetchId { get; private set; }
    public short StatusCode { get; private set; }
    public string? ErrorMessage { get; private set; }

    public ForecastFetch? ForecastFetch { get; private set; }

    public static FetchLog Create(int forecastFetchId, short statusCode, string? errorMessage = null)
    {
        return new FetchLog(forecastFetchId, statusCode, errorMessage);
    }
}
