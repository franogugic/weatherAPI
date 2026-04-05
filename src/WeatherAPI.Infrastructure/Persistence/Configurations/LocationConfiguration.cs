using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Infrastructure.Persistence.Configurations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("Location");

        builder.HasKey(location => location.Id)
            .HasName("PK_Location");

        builder.Property(location => location.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(location => location.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(100)")
            .HasMaxLength(100);

        builder.Property(location => location.Latitude)
            .HasColumnName("latitude")
            .HasPrecision(8, 6);

        builder.Property(location => location.Longitude)
            .HasColumnName("longitude")
            .HasPrecision(9, 6);

        builder.Property(location => location.Altitude)
            .HasColumnName("altitude");

        builder.HasIndex(location => new
            {
                location.Latitude,
                location.Longitude,
                location.Altitude
            })
            .IsUnique()
            .HasDatabaseName("UQ_Location_Latitude_Longitude_Altitude");

        builder.HasMany(location => location.ForecastFetches)
            .WithOne(forecastFetch => forecastFetch.Location)
            .HasForeignKey(forecastFetch => forecastFetch.LocationId)
            .HasConstraintName("FK_ForecastFetch_Location");
    }
}
