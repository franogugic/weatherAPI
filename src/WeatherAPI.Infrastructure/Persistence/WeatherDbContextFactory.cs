using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WeatherAPI.Infrastructure.Persistence;

public class WeatherDbContextFactory : IDesignTimeDbContextFactory<WeatherDbContext>
{
    public WeatherDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<WeatherDbContext>();

        optionsBuilder.UseSqlServer(
            "Server=localhost,1433;Database=WeatherDB;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;");

        return new WeatherDbContext(optionsBuilder.Options);
    }
}
