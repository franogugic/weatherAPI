using Microsoft.EntityFrameworkCore;
using WeatherAPI.Application.Interfaces;
using WeatherAPI.Domain.Entities;
using WeatherAPI.Infrastructure.Persistence;

namespace WeatherAPI.Infrastructure.Repositories;

public class ForecastRepository : IForecastRepository
{
    private readonly WeatherDbContext _context;
    
    public ForecastRepository(WeatherDbContext context)
    {
        _context = context;
    }
    
    // log
    public async Task AddFetchLogAsync(FetchLog fetchLog, CancellationToken cancellationToken = default)
    {
        await _context.FetchLogs.AddAsync(fetchLog, cancellationToken); 
    }
    
    // forecast fetch
    public async Task AddForecastFetchAsync(ForecastFetch forecastFetch, CancellationToken cancellationToken = default)
    {
        await _context.ForecastFetches.AddAsync(forecastFetch, cancellationToken);
    }
    //unit
    public async Task<List<Unit>> GetUnitsByValuesAsync(IEnumerable<string> values, CancellationToken cancellationToken = default)
    {
        var distinctValues = values.Distinct().ToList();

        return await _context.Units
            .Where(unit => distinctValues.Contains(unit.Value))
            .ToListAsync(cancellationToken);
    }
    
    public async Task AddUnitAsync(Unit unit, CancellationToken cancellationToken = default)
    {
        await _context.Units.AddAsync(unit, cancellationToken);
    }
    //metric
    public async Task<List<Metric>> GetMetricsByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken = default) 
    {
        var distinctNames = names.Distinct().ToList();

        return await _context.Metrics
            .Where(metric => distinctNames.Contains(metric.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task AddMetricAsync(Metric metric, CancellationToken cancellationToken = default)
    {
        await _context.Metrics.AddAsync(metric, cancellationToken);
    }
    //forecast fetch unit
    public async Task AddForecastFetchUnitAsync(ForecastFetchUnit forecastFetchUnit,
        CancellationToken cancellationToken = default)
    {
        await _context.ForecastFetchUnits.AddAsync(forecastFetchUnit, cancellationToken);
    }
    //weather symbol
    public async Task<List<WeatherSymbol>> GetWeatherSymbolsByCodesAsync(IEnumerable<string> codes, CancellationToken cancellationToken = default)
    {
        var distinctCodes = codes.Distinct().ToList();

        return await _context.WeatherSymbols
            .Where(symbol => distinctCodes.Contains(symbol.Code))
            .ToListAsync(cancellationToken);
    }
    public async Task AddWeatherSymbolAsync(WeatherSymbol weatherSymbol, CancellationToken cancellationToken = default)
    {
        await _context.WeatherSymbols.AddAsync(weatherSymbol, cancellationToken);
    }
    //hourly forecast
    public Task AddHourlyForecastsAsync(
        IEnumerable<HourlyForecast> hourlyForecasts,
        CancellationToken cancellationToken = default)
    {
        _context.HourlyForecasts.AddRange(hourlyForecasts);
        return Task.CompletedTask;
    }
    
    //spremanje
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    // posljednji fetch za lokaciju
    public async Task<ForecastFetch?> GetLatestFetchByLocationAsync(short locationId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ForecastFetches
            .AsNoTracking()
            .Where(fetch => fetch.LocationId == locationId)
            .OrderByDescending(fetch => fetch.FetchedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    //transakcija
    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        { 
            await operation(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
