namespace WeatherAPI.Domain.Entities;

public class ForecastFetchUnit
{
    private ForecastFetchUnit()
    {
    }

    private ForecastFetchUnit(int forecastFetchId, byte metricId, byte unitId)
    {
        ForecastFetchId = forecastFetchId;
        MetricId = metricId;
        UnitId = unitId;
    }

    public int ForecastFetchId { get; private set; }
    public byte MetricId { get; private set; }
    public byte UnitId { get; private set; }

    public ForecastFetch? ForecastFetch { get; private set; }
    public Metric? Metric { get; private set; }
    public Unit? Unit { get; private set; }

    public static ForecastFetchUnit Create(int forecastFetchId, byte metricId, byte unitId)
    {
        return new ForecastFetchUnit(forecastFetchId, metricId, unitId);
    }
}
