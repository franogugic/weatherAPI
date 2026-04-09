using System.Globalization;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeatherAPI.Application.Interfaces;
using WeatherAPI.Application.Models;
using WeatherAPI.Infrastructure.Configuration;

namespace WeatherAPI.Infrastructure.Services;

public class WeatherForecastApiClient(
    HttpClient httpClient,
    ILogger<WeatherForecastApiClient> logger,
    IOptions<WeatherApiOptions> weatherApiOptions) : IWeatherForecastApiClient
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
        // pripremanje rpodataka i kreiranje req
        var requestUri = BuildRequestUri(latitude, longitude, altitude);

        // slanje get req-a na forecast endpoint s retry-em
        try
        {
            logger.LogInformation(
                "Sending MET API forecast request for latitude {Latitude}, longitude {Longitude}, altitude {Altitude}.",
                latitude,
                longitude,
                altitude);

            using var response = await httpClient.GetAsync(requestUri, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            
            // sve 4xx i 5xx greske
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "MET API request failed with status code {StatusCode}.",
                    (short)response.StatusCode);

                return CreateApiErrorResponse(response, responseContent);
            }
            
            // deserijaliziacija 
            var forecastResponse =
                JsonSerializer.Deserialize<MetForecastResponse>(
                    responseContent,
                    JsonSerializerOptions);

            // provjera deserijalizacije
            if (forecastResponse is null)
            {
                logger.LogWarning("MET API returned an empty or invalid response payload.");

                return new ForecastApiResponse
                {
                    StatusCode = (short)response.StatusCode,
                    ErrorMessage = "MET API returned an empty or invalid response."
                };
            }

            // happy path za fetch 
            logger.LogInformation(
                "MET forecast response deserialized successfully with {TimeseriesCount} timeseries entries.",
                forecastResponse.Properties.Timeseries.Count);

            return new ForecastApiResponse
            {
                StatusCode = (short)response.StatusCode,
                ForecastResponse = forecastResponse
            };
        }
        // request timeoutan
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("MET API request timed out after all retry attempts.");

            return CreateErrorResponse(HttpStatusCode.GatewayTimeout, "MET API request timed out.");
        }
        // greska na mrezi
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "MET API request failed after all retry attempts.");

            return CreateErrorResponse(
                HttpStatusCode.ServiceUnavailable,
                "MET API request failed due to a network error.");
        }
    }

    // req urk
    private string BuildRequestUri(decimal latitude, decimal longitude, short? altitude)
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

        return $"{weatherApiOptions.Value.ForecastPath}?{string.Join("&", queryParameters)}";
    }

    // kreiranje errora kad imamo response od servera
    private static ForecastApiResponse CreateApiErrorResponse(HttpResponseMessage response, string responseContent)
    {
        return CreateErrorResponse(
            response.StatusCode,
            string.IsNullOrWhiteSpace(responseContent)
                ? response.ReasonPhrase
                : responseContent);
    }

    // error kad nije req stigao do servera
    private static ForecastApiResponse CreateErrorResponse(HttpStatusCode statusCode, string? errorMessage)
    {
        return new ForecastApiResponse
        {
            StatusCode = (short)statusCode,
            ErrorMessage = errorMessage
        };
    }
}
