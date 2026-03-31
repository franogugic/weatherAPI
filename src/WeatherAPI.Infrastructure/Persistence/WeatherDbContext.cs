using Microsoft.EntityFrameworkCore;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Infrastructure.Persistence;

public class WeatherDbContext(DbContextOptions<WeatherDbContext> options) : DbContext(options)
{
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<ForecastFetch> ForecastFetches => Set<ForecastFetch>();
    public DbSet<HourlyForecast> HourlyForecasts => Set<HourlyForecast>();
    public DbSet<FetchLog> FetchLogs => Set<FetchLog>();
    public DbSet<Metric> Metrics => Set<Metric>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<WeatherSymbol> WeatherSymbols => Set<WeatherSymbol>();
    public DbSet<ForecastFetchUnit> ForecastFetchUnits => Set<ForecastFetchUnit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WeatherDbContext).Assembly);
    }
}
