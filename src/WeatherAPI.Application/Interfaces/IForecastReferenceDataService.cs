using WeatherAPI.Application.Models;

namespace WeatherAPI.Application.Interfaces;

public interface IForecastReferenceDataService
{
    Task<ForecastReferenceData> PrepareReferenceDataAsync(
        MetForecastResponse forecastResponse,
        CancellationToken cancellationToken = default);
}
