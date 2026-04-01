using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WeatherAPI.Application.Interfaces;
using WeatherAPI.Application.Service;
using WeatherAPI.Infrastructure.Persistence;
using WeatherAPI.Infrastructure.Services;

namespace WeatherAPI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("WeatherDb");

        services.AddDbContext<WeatherDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddHttpClient<IWeatherForecastApiClient, WeatherForecastApiClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.met.no/");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("WeatherAPI/1.0");
        });

        services.AddScoped<IWeatherForecastService, WeatherForecastService>();

        
        return services;
    }
}
