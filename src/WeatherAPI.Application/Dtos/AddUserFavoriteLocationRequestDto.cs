using System.ComponentModel.DataAnnotations;

namespace WeatherAPI.Application.Dtos;

public class AddUserFavoriteLocationRequestDto
{
    [Range(1, short.MaxValue, ErrorMessage = "LocationId must be a positive number.")]
    public short LocationId { get; set; }
}
