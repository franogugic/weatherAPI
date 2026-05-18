namespace WeatherAPI.Application.Dtos;

public class GetLocationFetchLogResponseDto
{
    public int FetchId { get; set; }
    public short LocationId { get; set; }
    public string ResponseType { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public DateTime FetchedAt { get; set; }
    public short? StatusCode { get; set; }
    public string? ErrorMessage { get; set; }
    public int HourlyForecastCount { get; set; }
}
