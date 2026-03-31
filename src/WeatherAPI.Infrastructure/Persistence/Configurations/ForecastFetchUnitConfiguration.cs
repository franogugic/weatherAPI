using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Infrastructure.Persistence.Configurations;

public class ForecastFetchUnitConfiguration : IEntityTypeConfiguration<ForecastFetchUnit>
{
    public void Configure(EntityTypeBuilder<ForecastFetchUnit> builder)
    {
        builder.ToTable("ForecastFetchUnit");

        builder.HasKey(forecastFetchUnit => new { forecastFetchUnit.ForecastFetchId, forecastFetchUnit.MetricId })
            .HasName("PK_ForecastFetchUnit");

        builder.Property(forecastFetchUnit => forecastFetchUnit.ForecastFetchId)
            .HasColumnName("forecast_fetch_id");

        builder.Property(forecastFetchUnit => forecastFetchUnit.MetricId)
            .HasColumnName("metric_id");

        builder.Property(forecastFetchUnit => forecastFetchUnit.UnitId)
            .HasColumnName("unit_id");

        builder.HasOne(forecastFetchUnit => forecastFetchUnit.Metric)
            .WithMany(metric => metric.ForecastFetchUnits)
            .HasForeignKey(forecastFetchUnit => forecastFetchUnit.MetricId)
            .HasConstraintName("FK_ForecastFetchUnit_Metric");

        builder.HasOne(forecastFetchUnit => forecastFetchUnit.Unit)
            .WithMany(unit => unit.ForecastFetchUnits)
            .HasForeignKey(forecastFetchUnit => forecastFetchUnit.UnitId)
            .HasConstraintName("FK_ForecastFetchUnit_Unit");
    }
}
