using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeatherAPI.Infrastructure.Configuration;

namespace WeatherAPI.Infrastructure.Services;

public sealed class RetryOnTransientFailureHandler(
    IOptions<WeatherApiOptions> options,
    ILogger<RetryOnTransientFailureHandler> logger) : DelegatingHandler
{
    private readonly WeatherApiOptions _options = options.Value;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= _options.MaxRetryAttempts; attempt++)
        {
            var requestClone = await CloneHttpRequestMessageAsync(request, cancellationToken);

            try
            {
                var response = await base.SendAsync(requestClone, cancellationToken);

                if (ShouldRetry(response.StatusCode, attempt))
                {
                    logger.LogWarning(
                        "External API request attempt {Attempt}/{MaxAttempts} failed with transient status code {StatusCode}. Retrying.",
                        attempt,
                        _options.MaxRetryAttempts,
                        (short)response.StatusCode);

                    response.Dispose();
                    await DelayBeforeRetryAsync(cancellationToken);
                    continue;
                }

                return response;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && attempt < _options.MaxRetryAttempts)
            {
                logger.LogWarning(
                    "External API request attempt {Attempt}/{MaxAttempts} timed out after {TimeoutSeconds} seconds. Retrying.",
                    attempt,
                    _options.MaxRetryAttempts,
                    _options.TimeoutSeconds);

                await DelayBeforeRetryAsync(cancellationToken);
            }
            catch (HttpRequestException exception) when (attempt < _options.MaxRetryAttempts)
            {
                logger.LogWarning(
                    exception,
                    "External API request attempt {Attempt}/{MaxAttempts} failed due to a network error. Retrying.",
                    attempt,
                    _options.MaxRetryAttempts);

                await DelayBeforeRetryAsync(cancellationToken);
            }
        }

        throw new HttpRequestException("External API request failed after all retry attempts.");
    }

    private bool ShouldRetry(HttpStatusCode statusCode, int attempt)
    {
        if (attempt >= _options.MaxRetryAttempts)
        {
            return false;
        }

        var numericStatusCode = (int)statusCode;

        return statusCode == HttpStatusCode.RequestTimeout
            || statusCode == HttpStatusCode.TooManyRequests
            || numericStatusCode >= 500;
    }

    private Task DelayBeforeRetryAsync(CancellationToken cancellationToken)
    {
        return Task.Delay(TimeSpan.FromMilliseconds(_options.RetryDelayMilliseconds), cancellationToken);
    }

    private static async Task<HttpRequestMessage> CloneHttpRequestMessageAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version,
            VersionPolicy = request.VersionPolicy
        };

        foreach (var option in request.Options)
        {
            clone.Options.Set(new HttpRequestOptionsKey<object?>(option.Key), option.Value);
        }

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (request.Content is not null)
        {
            var memoryStream = new MemoryStream();
            await request.Content.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;

            var streamContent = new StreamContent(memoryStream);

            foreach (var header in request.Content.Headers)
            {
                streamContent.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            clone.Content = streamContent;
        }

        return clone;
    }
}
