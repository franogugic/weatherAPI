using WeatherAPI.Application.Dtos;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Application.Interfaces;

public interface IForecastRepository
{
    Task AddFetchLogAsync(FetchLog fetchLog, CancellationToken cancellationToken = default);
    Task AddForecastFetchAsync(ForecastFetch forecastFetch, CancellationToken cancellationToken = default);
    Task<List<Unit>> GetUnitsByValuesAsync(IEnumerable<string> values, CancellationToken cancellationToken = default);
    Task AddUnitAsync(Unit unit, CancellationToken cancellationToken = default);
    Task<List<Metric>> GetMetricsByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken = default);
    Task AddMetricAsync(Metric metric, CancellationToken cancellationToken = default);
    Task AddForecastFetchUnitAsync(ForecastFetchUnit forecastFetchUnit, CancellationToken cancellationToken = default);
    Task<List<WeatherSymbol>> GetWeatherSymbolsByCodesAsync(IEnumerable<string> codes, CancellationToken cancellationToken = default);
    Task AddWeatherSymbolAsync(WeatherSymbol weatherSymbol, CancellationToken cancellationToken = default);
    Task AddHourlyForecastsAsync(IEnumerable<HourlyForecast> hourlyForecasts, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default);
    Task<ForecastFetch?> GetLatestFetchByLocationAsync(short locationId, CancellationToken cancellationToken = default);

    Task<GetWeatherForecastQueryResultDto?> GetHourlyForecastsAsync(short locationId, int days,
        CancellationToken cancellationToken = default);
    
    Task<List<GetWeatherForecastUnitMetaQueryDto>> GetUnitsByFetchAsync(int fetchId, CancellationToken cancellationToken = default);

}
