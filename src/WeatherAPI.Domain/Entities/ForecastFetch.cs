namespace WeatherAPI.Domain.Entities;

public class ForecastFetch
{
    private ForecastFetch()
    {
    }

    private ForecastFetch(Location location, string responseType, DateTime? updatedAt, DateTime fetchedAt)
    {
        Location = location;
        ResponseType = responseType;
        UpdatedAt = updatedAt;
        FetchedAt = fetchedAt;
    }

    public int Id { get; private set; }
    public short LocationId { get; private set; }
    public string ResponseType { get; private set; } = string.Empty;
    public DateTime? UpdatedAt { get; private set; }
    public DateTime FetchedAt { get; private set; }

    public Location? Location { get; private set; }
    public ICollection<HourlyForecast> HourlyForecasts { get; private set; } = new List<HourlyForecast>();
    public FetchLog? FetchLog { get; private set; }
    public ICollection<ForecastFetchUnit> ForecastFetchUnits { get; private set; } = new List<ForecastFetchUnit>();

    public static ForecastFetch Create(Location location, string responseType, DateTime? updatedAt, DateTime fetchedAt)
    {
        return new ForecastFetch(location, responseType, updatedAt, fetchedAt);
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
