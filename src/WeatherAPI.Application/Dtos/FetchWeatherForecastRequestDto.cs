using System.ComponentModel.DataAnnotations;

namespace WeatherAPI.Application.Dtos;

public class FetchWeatherForecastRequestDto
{
    [Required(ErrorMessage = "Latitude is required.")]
    [Range(typeof(decimal), "-90", "90", ErrorMessage = "Latitude must be between -90 and 90.")]
    public decimal? Latitude { get; set; }

    [Required(ErrorMessage = "Longitude is required.")]
    [Range(typeof(decimal), "-180", "180", ErrorMessage = "Longitude must be between -180 and 180.")]
    public decimal? Longitude { get; set; }

    [Range(-500, 9000, ErrorMessage = "Altitude must be between -500 and 9000 meters.")]
    public short? Altitude { get; set; }
}
