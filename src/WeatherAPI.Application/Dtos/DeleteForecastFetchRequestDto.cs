using System.ComponentModel.DataAnnotations;

namespace WeatherAPI.Application.Dtos;

public class DeleteForecastFetchRequestDto
{
    [Range(1, int.MaxValue, ErrorMessage = "FetchId must be a positive number.")]
    public int FetchId { get; set; }

}