using System.ComponentModel.DataAnnotations;

namespace WeatherAPI.Application.Dtos;

public class RegisterUserRequestDto
{
    [Required]
    [MinLength(2)]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    [MinLength(2)]
    [MaxLength(50)]
    public string LastName { get; set; }  = string.Empty;
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
}