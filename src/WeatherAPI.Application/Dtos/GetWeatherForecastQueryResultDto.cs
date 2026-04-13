namespace WeatherAPI.Application.Dtos;

// query za dohvat hourlya response
// dodan da ne vracamo fetchId za svaki zapis neg jednon samo
public class GetWeatherForecastQueryResultDto
{
    public int ForecastFetchId { get; set; }
    public List<GetWeatherForecastItemDto> Items { get; set; } = [];
}
