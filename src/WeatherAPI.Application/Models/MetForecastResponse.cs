using System.Text.Json.Serialization;

namespace WeatherAPI.Application.Models;

public class MetForecastResponse
{
    public MetForecastProperties Properties { get; set; } = new();
}

public class MetForecastProperties
{
    public MetForecastMeta Meta { get; set; } = new();
    public List<MetForecastTimeseries> Timeseries { get; set; } = [];
}

public class MetForecastTimeseries
{
    public DateTime Time { get; set; }
    public MetForecastData Data { get; set; } = new();
}

public class MetForecastMeta
{
    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; set; }

    public Dictionary<string, string> Units { get; set; } = [];
}

public class MetForecastData
{
    public MetForecastInstant Instant { get; set; } = new();

    [JsonPropertyName("next_1_hours")]
    public MetForecastNextPeriod? Next1Hours { get; set; }

    [JsonPropertyName("next_6_hours")]
    public MetForecastNextPeriod? Next6Hours { get; set; }

    [JsonPropertyName("next_12_hours")]
    public MetForecastNextPeriod? Next12Hours { get; set; }
}

public class MetForecastInstant
{
    public MetForecastInstantDetails Details { get; set; } = new();
}

public class MetForecastInstantDetails
{
    [JsonPropertyName("air_temperature")]
    public decimal? AirTemperature { get; set; }

    [JsonPropertyName("air_pressure_at_sea_level")]
    public decimal? AirPressureAtSeaLevel { get; set; }

    [JsonPropertyName("cloud_area_fraction")]
    public decimal? CloudAreaFraction { get; set; }

    [JsonPropertyName("relative_humidity")]
    public decimal? RelativeHumidity { get; set; }

    [JsonPropertyName("wind_from_direction")]
    public decimal? WindFromDirection { get; set; }

    [JsonPropertyName("wind_speed")]
    public decimal? WindSpeed { get; set; }
}

public class MetForecastNextPeriod
{
    public MetForecastSummary? Summary { get; set; }
    public MetForecastNextPeriodDetails? Details { get; set; }
}

public class MetForecastSummary
{
    [JsonPropertyName("symbol_code")]
    public string? SymbolCode { get; set; }
}

public class MetForecastNextPeriodDetails
{
    [JsonPropertyName("precipitation_amount")]
    public decimal? PrecipitationAmount { get; set; }
}
