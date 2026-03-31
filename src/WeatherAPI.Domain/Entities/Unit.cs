namespace WeatherAPI.Domain.Entities;

public class Unit
{
    private Unit()
    {
    }

    private Unit(string value, string displayName, string? description)
    {
        Value = value;
        DisplayName = displayName;
        Description = description;
    }

    public byte Id { get; private set; }
    public string Value { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public ICollection<ForecastFetchUnit> ForecastFetchUnits { get; private set; } = new List<ForecastFetchUnit>();

    public static Unit Create(string value, string displayName, string? description = null)
    {
        return new Unit(value, displayName, description);
    }
}
