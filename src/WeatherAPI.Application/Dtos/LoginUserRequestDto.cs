using System.ComponentModel.DataAnnotations;

namespace WeatherAPI.Application.Dtos;

public class LoginUserRequestDto
{
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
}