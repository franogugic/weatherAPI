using System.ComponentModel.DataAnnotations;

namespace WeatherAPI.Application.Dtos;

public class UpdateUserPreferenceRequestDto
{
    [Required]
    [MaxLength(30)]
    public string TemperatureUnit { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string WindSpeedUnit { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string PressureUnit { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string CloudinessUnit { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string PrecipitationUnit { get; set; } = string.Empty;
}