using System.ComponentModel.DataAnnotations;

namespace WeatherAPI.Application.Dtos;

public class GetWeatherForecastRequestDto
{
    [Range(1, 10, ErrorMessage = "Days must be between 1 and 10.")]
    public int Days { get; set; } = 3;
    [Range(1, short.MaxValue, ErrorMessage = "LocationId must be a positive number.")]
    public short LocationId { get; set; }
}
