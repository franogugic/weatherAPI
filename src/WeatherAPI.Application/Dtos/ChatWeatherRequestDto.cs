using System.ComponentModel.DataAnnotations;

namespace WeatherAPI.Application.Dtos;

public class ChatWeatherRequestDto
{
    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public string Message { get; set; } = string.Empty;

    [Range(1, short.MaxValue)]
    public short LocationId { get; set; }

    [StringLength(10)]
    public string? Language { get; set; }
}
