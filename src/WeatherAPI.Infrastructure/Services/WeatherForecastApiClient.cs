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

    public async Task<ForecastApiResponse> FetchForecastAsync(
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
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning(
                "MET API request failed with status code {StatusCode}.",
                (short)response.StatusCode);

            return new ForecastApiResponse
            {
                StatusCode = (short)response.StatusCode,
                ErrorMessage = string.IsNullOrWhiteSpace(responseContent)
                    ? response.ReasonPhrase
                    : responseContent
            };
        }

        var forecastResponse =
            JsonSerializer.Deserialize<MetForecastResponse>(
                responseContent,
                JsonSerializerOptions);

        if (forecastResponse is null)
        {
            logger.LogWarning("MET API returned an empty or invalid response payload.");

            return new ForecastApiResponse
            {
                StatusCode = (short)response.StatusCode,
                ErrorMessage = "MET API returned an empty or invalid response."
            };
        }

        logger.LogInformation(
            "MET forecast response deserialized successfully with {TimeseriesCount} timeseries entries.",
            forecastResponse.Properties.Timeseries.Count);

        return new ForecastApiResponse
        {
            StatusCode = (short)response.StatusCode,
            ForecastResponse = forecastResponse
        };
    }
}
