namespace WeatherAPI.Domain.Entities;

public class Location
{
    private Location() { }

    private Location(decimal latitude, decimal longitude, short? altitude, string? name)
    {
        Latitude = latitude;
        Longitude = longitude;
        Altitude = altitude;
        Name = name;
    }

    public short Id { get; private set; }
    public string? Name { get; private set; }
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }
    public short? Altitude { get; private set; }

    public ICollection<ForecastFetch> ForecastFetches { get; private set; } = new List<ForecastFetch>();
    
    public static Location Create(decimal latitude, decimal longitude, short? altitude, string? name = null)
    {
        return new Location(latitude, longitude, altitude, name);
    }

    public void Rename(string? name)
    {
        Name = name;
    }
}
