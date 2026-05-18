using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using WeatherAPI.Infrastructure.Configuration;

namespace WeatherAPI.Infrastructure.Persistence;

public class UserDbContextFactory : IDesignTimeDbContextFactory<UserDbContext>
{
    public UserDbContext CreateDbContext(string[] args)
    {
        var configuration = DesignTimeConfiguration.Build();
        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
        var connectionString = configuration.GetConnectionString("UserDb")
            ?? throw new InvalidOperationException(
                "UserDb connection string is not configured. Set 'ConnectionStrings:UserDb' in appsettings or 'ConnectionStrings__UserDb' in the environment/.env file.");
        
        optionsBuilder.UseNpgsql(connectionString);
        
        return new UserDbContext(optionsBuilder.Options);
    }
}
