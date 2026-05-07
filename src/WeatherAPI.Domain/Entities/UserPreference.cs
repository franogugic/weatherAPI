namespace WeatherAPI.Domain.Entities;

public class UserPreference
{
    private UserPreference()
    {
    }

    private UserPreference(
        User user,
        string temperatureUnit,
        string windSpeedUnit,
        string pressureUnit,
        string cloudinessUnit,
        string precipitationUnit)
    {
        User = user;
        UserId = user.Id;
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
        User user,
        string temperatureUnit,
        string windSpeedUnit,
        string pressureUnit,
        string cloudinessUnit,
        string precipitationUnit)
    {
        return new UserPreference(
            user,
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
