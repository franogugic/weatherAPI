using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using WeatherAPI.Infrastructure.Configuration;

namespace WeatherAPI.Infrastructure.Persistence;

public class WeatherDbContextFactory : IDesignTimeDbContextFactory<WeatherDbContext>
{
    public WeatherDbContext CreateDbContext(string[] args)
    {
        var configuration = DesignTimeConfiguration.Build();
        var optionsBuilder = new DbContextOptionsBuilder<WeatherDbContext>();
        var connectionString = configuration.GetConnectionString("WeatherDb")
            ?? throw new InvalidOperationException(
                "WeatherDb connection string is not configured. Set 'ConnectionStrings:WeatherDb' in appsettings or 'ConnectionStrings__WeatherDb' in the environment/.env file.");

        optionsBuilder.UseSqlServer(connectionString);

        return new WeatherDbContext(optionsBuilder.Options);
    }
}
