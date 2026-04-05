using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Infrastructure.Persistence.Configurations;

public class MetricConfiguration : IEntityTypeConfiguration<Metric>
{
    public void Configure(EntityTypeBuilder<Metric> builder)
    {
        builder.ToTable("Metric");

        builder.HasKey(metric => metric.Id)
            .HasName("PK_Metric");

        builder.HasIndex(metric => metric.Name)
            .IsUnique()
            .HasDatabaseName("UQ_Metric_Name");

        builder.Property(metric => metric.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(metric => metric.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(50)")
            .HasMaxLength(50)
            .IsUnicode(false);
    }
}
