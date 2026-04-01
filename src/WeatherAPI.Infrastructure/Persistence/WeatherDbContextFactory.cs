using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WeatherAPI.Infrastructure.Persistence;

public class WeatherDbContextFactory : IDesignTimeDbContextFactory<WeatherDbContext>
{
    public WeatherDbContext CreateDbContext(string[] args)
    {
        Env.Load();

        var optionsBuilder = new DbContextOptionsBuilder<WeatherDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__WeatherDb")
            ?? throw new InvalidOperationException("ConnectionStrings__WeatherDb is not configured.");

        optionsBuilder.UseSqlServer(connectionString);

        return new WeatherDbContext(optionsBuilder.Options);
    }
}
