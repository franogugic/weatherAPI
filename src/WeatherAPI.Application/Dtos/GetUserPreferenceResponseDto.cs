namespace WeatherAPI.Application.Dtos;

public class GetUserPreferenceResponseDto
{
    public string TemperatureUnit { get; set; } = string.Empty;
    public string WindSpeedUnit { get; set; } = string.Empty;
    public string PressureUnit { get; set; } = string.Empty;
    public string CloudinessUnit { get; set; } = string.Empty;
    public string PrecipitationUnit { get; set; } = string.Empty;
}
