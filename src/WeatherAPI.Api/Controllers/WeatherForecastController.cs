using Microsoft.AspNetCore.Mvc;
using WeatherAPI.Application.Dtos;
using WeatherAPI.Application.Interfaces;

namespace WeatherAPI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly IWeatherForecastService _weatherForecastService;

    public WeatherForecastController(IWeatherForecastService weatherForecastService)
    {
        _weatherForecastService = weatherForecastService;
    }
    
    [HttpPost]
    public async Task<IActionResult> Fetch(
        [FromBody] FetchWeatherForecastRequestDto request,
        CancellationToken cancellationToken)
    {
        var response = await _weatherForecastService.FetchWeatherForecastAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] GetWeatherForecastRequestDto request,
        CancellationToken cancellationToken)
    {
        var response = await _weatherForecastService.GetWeatherForecast(request, cancellationToken);
        return Ok(response);
    }
    
    [HttpDelete]
    public async Task<IActionResult> Delete(
        [FromQuery] DeleteForecastFetchRequestDto request,
        CancellationToken cancellationToken)
    {
        await _weatherForecastService.DeleteForecastFetchAsync(request, cancellationToken);
        return NoContent();
    }
    
    
}
