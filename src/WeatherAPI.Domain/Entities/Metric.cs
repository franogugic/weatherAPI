namespace WeatherAPI.Domain.Entities;

public class Metric
{
    private Metric()
    {
    }

    private Metric(string name)
    {
        Name = name;
    }

    public byte Id { get; private set; }
    public string Name { get; private set; } = string.Empty;

    public ICollection<ForecastFetchUnit> ForecastFetchUnits { get; private set; } = new List<ForecastFetchUnit>();

    public static Metric Create(string name)
    {
        return new Metric(name);
    }
}
