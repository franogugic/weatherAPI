
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WeatherAPI.Application.Dtos;
using WeatherAPI.Application.Interfaces;

namespace WeatherAPI.Infrastructure.Services;

public class ForecastFetchBackgroundService : BackgroundService
{
    private static readonly TimeSpan FetchInterval = TimeSpan.FromHours(2);

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ForecastFetchBackgroundService> _logger;
    
    public ForecastFetchBackgroundService(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ForecastFetchBackgroundService> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await FetchAllLocationsAsync(stoppingToken);

        using var timer = new PeriodicTimer(FetchInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await FetchAllLocationsAsync(stoppingToken);
        }
    }

    private async Task FetchAllLocationsAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var weatherForecastService = scope.ServiceProvider.GetRequiredService<IWeatherForecastService>();
            var locations = await weatherForecastService.GetLocationsAsync(stoppingToken);

            foreach (var location in locations)
            {
                try
                {
                    await weatherForecastService.FetchWeatherForecastAsync(
                        location,
                        stoppingToken);
                    _logger.LogInformation(
                        "Successfully fetched forecast for location with latitude {Latitude}, longitude {Longitude}, altitude {Altitude}.",
                        location.Latitude,
                        location.Longitude,
                        location.Altitude);
                }
                catch (Exception exception)
                {
                    _logger.LogError(
                        exception,
                        "Forecast fetch failed for location with latitude {Latitude}, longitude {Longitude}, altitude {Altitude}.",
                        location.Latitude,
                        location.Longitude,
                        location.Altitude);
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Background forecast fetch run failed.");
        }
    }
}
