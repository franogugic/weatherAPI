using Microsoft.EntityFrameworkCore;
using WeatherAPI.Application.Dtos;
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
            .Include(forecastFetch => forecastFetch.FetchLog)
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
    
    // dphvat po satu
    public async Task<GetWeatherForecastQueryResultDto?> GetHourlyForecastsAsync(
        short locationId,
        int days,
        CancellationToken cancellationToken = default)
    {
        var start = DateTime.UtcNow;
        // danasnji + broj dana
        var end = start.AddDays(days + 1).Date; // da ukljucimo citav zadnji dan

        return await _context.ForecastFetches
            .AsNoTracking()
            .Where(fetch => fetch.LocationId == locationId
                            && fetch.FetchLog != null
                            && fetch.FetchLog.StatusCode == 200
                            && fetch.HourlyForecasts.Any())
            .OrderByDescending(fetch => fetch.FetchedAt)
            // 
            .Select(fetch => new GetWeatherForecastQueryResultDto
            {
                // dto - koji vraca listu dto responsa i fetchId
                // fetchId radi citanje unita za drugi query
                ForecastFetchId = fetch.Id,
                Items = fetch.HourlyForecasts
                    .Where(hourly => hourly.ForecastTime >= start
                                     && hourly.ForecastTime < end)
                    .OrderBy(hourly => hourly.ForecastTime)
                    .Select(hourly => new GetWeatherForecastItemDto
                    {
                        ForecastTime = hourly.ForecastTime,
                        AirTemperature = hourly.AirTemperature,
                        AirPressureAtSeaLevel = hourly.AirPressureAtSeaLevel,
                        Cloudiness = hourly.Cloudiness,
                        Humidity = hourly.Humidity,
                        WindDirection = hourly.WindDirection,
                        WindSpeed = hourly.WindSpeed,
                        WeatherSymbol = hourly.WeatherSymbol.Code,
                        PrecipitationAmount = hourly.PrecipitationAmount
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
    
    //dohvat unita za fetch
    public Task<List<GetWeatherForecastUnitMetaQueryDto>> GetUnitByFetchAsync(int fetchId, CancellationToken cancellationToken = default)
    {
        return _context.ForecastFetchUnits
            .AsNoTracking()
            .Include(ffu => ffu.Unit)
            .Include(ffu => ffu.Metric)
            .Where(ffu => ffu.ForecastFetchId == fetchId )
            .Select(ffu => new GetWeatherForecastUnitMetaQueryDto
            {
                MetricName = ffu.Metric.Name,
                UnitDisplayName = ffu.Unit.DisplayName,
                UnitDescription = ffu.Unit.Description
            })
            .ToListAsync(cancellationToken);
    }

}
