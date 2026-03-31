using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Infrastructure.Persistence.Configurations;

public class HourlyForecastConfiguration : IEntityTypeConfiguration<HourlyForecast>
{
    public void Configure(EntityTypeBuilder<HourlyForecast> builder)
    {
        builder.ToTable("HourlyForecast");

        builder.HasKey(hourlyForecast => new { hourlyForecast.ForecastFetchId, hourlyForecast.ForecastTime })
            .HasName("PK_HourlyForecast");

        builder.Property(hourlyForecast => hourlyForecast.ForecastFetchId)
            .HasColumnName("forecast_fetch_id");

        builder.Property(hourlyForecast => hourlyForecast.ForecastTime)
            .HasColumnName("forecast_time")
            .HasColumnType("datetime2(0)");

        builder.Property(hourlyForecast => hourlyForecast.AirTemperature)
            .HasColumnName("air_temperature")
            .HasPrecision(4, 1);

        builder.Property(hourlyForecast => hourlyForecast.AirPressureAtSeaLevel)
            .HasColumnName("air_pressure_at_sea_level")
            .HasPrecision(6, 1);

        builder.Property(hourlyForecast => hourlyForecast.Cloudiness)
            .HasColumnName("cloudiness");

        builder.Property(hourlyForecast => hourlyForecast.Humidity)
            .HasColumnName("humidity");

        builder.Property(hourlyForecast => hourlyForecast.WindDirection)
            .HasColumnName("wind_direction")
            .HasPrecision(5, 2);

        builder.Property(hourlyForecast => hourlyForecast.WindSpeed)
            .HasColumnName("wind_speed")
            .HasPrecision(4, 1);

        builder.Property(hourlyForecast => hourlyForecast.WeatherSymbolId)
            .HasColumnName("weather_symbol_id");

        builder.Property(hourlyForecast => hourlyForecast.PrecipitationAmount)
            .HasColumnName("precipitation_amount")
            .HasPrecision(5, 2);

        builder.HasOne(hourlyForecast => hourlyForecast.WeatherSymbol)
            .WithMany(weatherSymbol => weatherSymbol.HourlyForecasts)
            .HasForeignKey(hourlyForecast => hourlyForecast.WeatherSymbolId)
            .HasConstraintName("FK_HourlyForecast_Symbol");
    }
}
