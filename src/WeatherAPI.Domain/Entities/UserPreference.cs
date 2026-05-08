namespace WeatherAPI.Domain.Entities;

public class UserPreference
{
    private UserPreference()
    {
    }

    private UserPreference(
        int userId,
        string temperatureUnit,
        string windSpeedUnit,
        string pressureUnit,
        string cloudinessUnit,
        string precipitationUnit)
    {
        UserId = userId;
        TemperatureUnit = temperatureUnit;
        WindSpeedUnit = windSpeedUnit;
        PressureUnit = pressureUnit;
        CloudinessUnit = cloudinessUnit;
        PrecipitationUnit = precipitationUnit;
        UpdatedAt = DateTime.UtcNow;
    }
    
    private UserPreference(
        string temperatureUnit,
        string windSpeedUnit,
        string pressureUnit,
        string cloudinessUnit,
        string precipitationUnit)
    {
        TemperatureUnit = temperatureUnit;
        WindSpeedUnit = windSpeedUnit;
        PressureUnit = pressureUnit;
        CloudinessUnit = cloudinessUnit;
        PrecipitationUnit = precipitationUnit;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public int UserId { get; private set; }
    public string TemperatureUnit { get; private set; } = string.Empty;
    public string WindSpeedUnit { get; private set; } = string.Empty;
    public string PressureUnit { get; private set; } = string.Empty;
    public string CloudinessUnit { get; private set; } = string.Empty;
    public string PrecipitationUnit { get; private set; } = string.Empty;
    public DateTime UpdatedAt { get; private set; }
    
    public User User { get; private set; } = null!;

    public static UserPreference Create(
        int userId,
        string temperatureUnit,
        string windSpeedUnit,
        string pressureUnit,
        string cloudinessUnit,
        string precipitationUnit)
    {
        return new UserPreference(
            userId,
            temperatureUnit,
            windSpeedUnit,
            pressureUnit,
            cloudinessUnit,
            precipitationUnit);
    }

    public void Update(
        string temperatureUnit,
        string windSpeedUnit,
        string pressureUnit,
        string cloudinessUnit,
        string precipitationUnit)
    {
        TemperatureUnit = temperatureUnit;
        WindSpeedUnit = windSpeedUnit;
        PressureUnit = pressureUnit;
        CloudinessUnit = cloudinessUnit;
        PrecipitationUnit = precipitationUnit;
        UpdatedAt = DateTime.UtcNow;
    }
}
