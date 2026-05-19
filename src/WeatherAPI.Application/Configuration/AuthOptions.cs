namespace WeatherAPI.Application.Configuration;

public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    public int SessionDurationDays { get; set; } = 7;
    public string SessionCookieName { get; set; } = "weather_session";
    public bool CookieSecure { get; set; }
    public string CookieSameSite { get; set; } = "Lax";
}
