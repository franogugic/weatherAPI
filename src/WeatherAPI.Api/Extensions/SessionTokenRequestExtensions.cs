using WeatherAPI.Application.Configuration;

namespace WeatherAPI.Api.Extensions;

public static class SessionTokenRequestExtensions
{
    private const string BearerPrefix = "Bearer ";

    public static string GetSessionToken(this HttpRequest request, AuthOptions authOptions)
    {
        var cookieToken = request.Cookies[authOptions.SessionCookieName];

        if (!string.IsNullOrWhiteSpace(cookieToken))
        {
            return cookieToken;
        }

        var authorizationHeader = request.Headers["Authorization"].ToString();

        if (authorizationHeader.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var bearerToken = authorizationHeader[BearerPrefix.Length..].Trim();

            if (!string.IsNullOrWhiteSpace(bearerToken))
            {
                return bearerToken;
            }
        }

        throw new UnauthorizedAccessException("User is not authenticated");
    }
}
