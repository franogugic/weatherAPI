using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Infrastructure.Persistence.Configurations;

public class FetchLogConfiguration : IEntityTypeConfiguration<FetchLog>
{
    public void Configure(EntityTypeBuilder<FetchLog> builder)
    {
        builder.ToTable("FetchLog");

        builder.HasKey(fetchLog => fetchLog.Id)
            .HasName("PK_FetchLog");

        builder.HasIndex(fetchLog => fetchLog.ForecastFetchId)
            .IsUnique()
            .HasDatabaseName("UQ_FetchLog_ForecastFetch");

        builder.Property(fetchLog => fetchLog.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(fetchLog => fetchLog.ForecastFetchId)
            .HasColumnName("forecast_fetch_id");

        builder.Property(fetchLog => fetchLog.StatusCode)
            .HasColumnName("status_code");

        builder.Property(fetchLog => fetchLog.ErrorMessage)
            .HasColumnName("error_message")
            .HasColumnType("varchar(1000)")
            .HasMaxLength(1000)
            .IsUnicode(false);
    }
}
