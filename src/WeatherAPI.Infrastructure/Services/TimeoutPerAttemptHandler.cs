using Microsoft.Extensions.Options;
using WeatherAPI.Infrastructure.Configuration;

namespace WeatherAPI.Infrastructure.Services;

public sealed class TimeoutPerAttemptHandler(IOptions<WeatherApiOptions> options) : DelegatingHandler
{
    private readonly WeatherApiOptions _options = options.Value;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        using var timeoutCancellationTokenSource =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // prekid requesta nakon 5 sek
        timeoutCancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

        return await base.SendAsync(request, timeoutCancellationTokenSource.Token);
    }
}
