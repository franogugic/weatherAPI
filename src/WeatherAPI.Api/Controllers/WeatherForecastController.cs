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
    
    [HttpGet]
    public async Task<IActionResult> Fetch(
        [FromQuery] FetchWeatherForecastRequestDto request,
        CancellationToken cancellationToken)
    {
        var response = await _weatherForecastService.FetchWeatherForecastAsync(request, cancellationToken);
        return Ok(response);
    }
}
