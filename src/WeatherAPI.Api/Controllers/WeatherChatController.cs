using Microsoft.AspNetCore.Mvc;
using WeatherAPI.Application.Dtos;
using WeatherAPI.Application.Interfaces;

namespace WeatherAPI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherChatController : ControllerBase
{
    private readonly IWeatherChatService _weatherChatService;

    public WeatherChatController(IWeatherChatService weatherChatService)
    {
        _weatherChatService = weatherChatService;
    }

    [HttpPost]
    public async Task<IActionResult> Ask(
        [FromBody] ChatWeatherRequestDto request,
        CancellationToken cancellationToken)
    {
        var response = await _weatherChatService.AskAsync(request, cancellationToken);
        return Ok(response);
    }
}
