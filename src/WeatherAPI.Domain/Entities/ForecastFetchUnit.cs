namespace WeatherAPI.Domain.Entities;

public class ForecastFetchUnit
{
    private ForecastFetchUnit()
    {
    }

    private ForecastFetchUnit(ForecastFetch forecastFetch, Metric metric, Unit unit)
    {
        ForecastFetch = forecastFetch;
        Metric = metric;
        Unit = unit;
    }

    public int ForecastFetchId { get; private set; }
    public byte MetricId { get; private set; }
    public byte UnitId { get; private set; }

    public ForecastFetch? ForecastFetch { get; private set; }
    public Metric? Metric { get; private set; }
    public Unit? Unit { get; private set; }

    public static ForecastFetchUnit Create(ForecastFetch forecastFetch, Metric metric, Unit unit)
    {
        return new ForecastFetchUnit(forecastFetch, metric, unit);
    }
}
