using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using WeatherAPI.Application.Interfaces;
using WeatherAPI.Application.Models;

namespace WeatherAPI.Infrastructure.Services;

public class WeatherForecastApiClient(HttpClient httpClient, ILogger<WeatherForecastApiClient> logger) : IWeatherForecastApiClient
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<MetForecastResponse> FetchForecastAsync(
        decimal latitude,
        decimal longitude,
        short? altitude,
        CancellationToken cancellationToken = default)
    {
        var queryParameters = new List<string>
        {
            $"lat={latitude.ToString(CultureInfo.InvariantCulture)}",
            $"lon={longitude.ToString(CultureInfo.InvariantCulture)}"
        };

        if (altitude.HasValue)
        {
            queryParameters.Add($"altitude={altitude.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        var requestUri = $"weatherapi/locationforecast/2.0/compact?{string.Join("&", queryParameters)}";

        using var response = await httpClient.GetAsync(requestUri, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);

        var forecastResponse =
            await JsonSerializer.DeserializeAsync<MetForecastResponse>(
                responseStream,
                JsonSerializerOptions,
                cancellationToken);

        if (forecastResponse is null)
        {
            logger.LogWarning("MET API returned an empty or invalid response payload.");
            throw new InvalidOperationException("MET API returned an empty or invalid response.");
        }

        logger.LogInformation(
            "MET forecast response deserialized successfully with {TimeseriesCount} timeseries entries.",
            forecastResponse.Properties.Timeseries.Count);

        return forecastResponse;
    }
}
