using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using WeatherAPI.Infrastructure.Configuration;

namespace WeatherAPI.Infrastructure.Persistence;

public class WeatherDbContextFactory : IDesignTimeDbContextFactory<WeatherDbContext>
{
    public WeatherDbContext CreateDbContext(string[] args)
    {
        EnvironmentLoader.LoadFromRoot();

        var optionsBuilder = new DbContextOptionsBuilder<WeatherDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__WeatherDb")
            ?? throw new InvalidOperationException("ConnectionStrings__WeatherDb is not configured.");

        optionsBuilder.UseSqlServer(connectionString);

        return new WeatherDbContext(optionsBuilder.Options);
    }
}
