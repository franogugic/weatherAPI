using System.ComponentModel.DataAnnotations;

namespace WeatherAPI.Application.Dtos;

public class CreateLocationRequestDto
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Range(-90, 90)]
    public decimal Latitude { get; set; }

    [Range(-180, 180)]
    public decimal Longitude { get; set; }

    public short? Altitude { get; set; }
}
