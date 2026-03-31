namespace WeatherAPI.Domain.Entities;

public class ForecastFetch
{
    private ForecastFetch()
    {
    }

    private ForecastFetch(short locationId, string responseType, DateTime updatedAt, DateTime fetchedAt)
    {
        LocationId = locationId;
        ResponseType = responseType;
        UpdatedAt = updatedAt;
        FetchedAt = fetchedAt;
    }

    public int Id { get; private set; }
    public short LocationId { get; private set; }
    public string ResponseType { get; private set; } = string.Empty;
    public DateTime UpdatedAt { get; private set; }
    public DateTime FetchedAt { get; private set; }

    public Location Location { get; private set; }
    public ICollection<HourlyForecast> HourlyForecasts { get; private set; } = new List<HourlyForecast>();
    public FetchLog? FetchLog { get; private set; }
    public ICollection<ForecastFetchUnit> ForecastFetchUnits { get; private set; } = new List<ForecastFetchUnit>();

    public static ForecastFetch Create(short locationId, string responseType, DateTime updatedAt, DateTime fetchedAt)
    {
        return new ForecastFetch(locationId, responseType, updatedAt, fetchedAt);
    }

    public void AddHourlyForecast(HourlyForecast hourlyForecast)
    {
        HourlyForecasts.Add(hourlyForecast);
    }

    public void SetFetchLog(FetchLog fetchLog)
    {
        FetchLog = fetchLog;
    }

    public void AddForecastFetchUnit(ForecastFetchUnit forecastFetchUnit)
    {
        ForecastFetchUnits.Add(forecastFetchUnit);
    }
}
