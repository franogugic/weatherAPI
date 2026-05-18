using System.ComponentModel.DataAnnotations;

namespace WeatherAPI.Application.Dtos;

public class UpdateUserDashboardLayoutRequestDto
{
    [Required]
    public string LayoutJson { get; set; } = string.Empty;
}
