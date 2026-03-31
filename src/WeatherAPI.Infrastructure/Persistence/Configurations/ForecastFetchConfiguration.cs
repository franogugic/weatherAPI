using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Infrastructure.Persistence.Configurations;

public class ForecastFetchConfiguration : IEntityTypeConfiguration<ForecastFetch>
{
    public void Configure(EntityTypeBuilder<ForecastFetch> builder)
    {
        builder.ToTable("ForecastFetch");

        builder.HasKey(forecastFetch => forecastFetch.Id)
            .HasName("PK_ForecastFetch");

        builder.Property(forecastFetch => forecastFetch.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(forecastFetch => forecastFetch.LocationId)
            .HasColumnName("location_id");

        builder.Property(forecastFetch => forecastFetch.ResponseType)
            .HasColumnName("response_type");

        builder.Property(forecastFetch => forecastFetch.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime2(0)");

        builder.Property(forecastFetch => forecastFetch.FetchedAt)
            .HasColumnName("fetched_at")
            .HasColumnType("datetime2(0)");

        builder.HasMany(forecastFetch => forecastFetch.HourlyForecasts)
            .WithOne(hourlyForecast => hourlyForecast.ForecastFetch)
            .HasForeignKey(hourlyForecast => hourlyForecast.ForecastFetchId)
            .HasConstraintName("FK_HourlyForecast_Fetch");

        builder.HasOne(forecastFetch => forecastFetch.FetchLog)
            .WithOne(fetchLog => fetchLog.ForecastFetch)
            .HasForeignKey<FetchLog>(fetchLog => fetchLog.ForecastFetchId)
            .HasConstraintName("FK_FetchLog_ForecastFetch");

        builder.HasMany(forecastFetch => forecastFetch.ForecastFetchUnits)
            .WithOne(forecastFetchUnit => forecastFetchUnit.ForecastFetch)
            .HasForeignKey(forecastFetchUnit => forecastFetchUnit.ForecastFetchId)
            .HasConstraintName("FK_ForecastFetchUnit_Fetch");
    }
}
