namespace WeatherAPI.Domain.Constants;

public static class UserPreferenceUnits
{
    public const string Celsius = "celsius";
    public const string Fahrenheit = "fahrenheit";
    public const string Kelvin = "kelvin";

    public const string MetersPerSecond = "metersPerSecond";
    public const string KilometersPerHour = "kilometersPerHour";
    public const string MilesPerHour = "milesPerHour";
    public const string Knots = "knots";

    public const string Hectopascal = "hectopascal";
    public const string Pascal = "pascal";
    public const string Millibar = "millibar";

    public const string Percent = "percent";
    public const string Okta = "okta";

    public const string Millimeter = "millimeter";
    public const string LiterPerSquareMeter = "literPerSquareMeter";

    public static readonly HashSet<string> TemperatureUnits =
    [
        Celsius,
        Fahrenheit,
        Kelvin
    ];

    public static readonly HashSet<string> WindSpeedUnits =
    [
        MetersPerSecond,
        KilometersPerHour,
        MilesPerHour,
        Knots
    ];

    public static readonly HashSet<string> PressureUnits =
    [
        Hectopascal,
        Pascal,
        Millibar
    ];

    public static readonly HashSet<string> CloudinessUnits =
    [
        Percent,
        Okta
    ];

    public static readonly HashSet<string> PrecipitationUnits =
    [
        Millimeter,
        LiterPerSquareMeter
    ];
}