using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WeatherAPI.Infrastructure.Persistence;

namespace WeatherAPI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("WeatherDb");

        services.AddDbContext<WeatherDbContext>(options =>
            options.UseSqlServer(connectionString));

        return services;
    }
}
