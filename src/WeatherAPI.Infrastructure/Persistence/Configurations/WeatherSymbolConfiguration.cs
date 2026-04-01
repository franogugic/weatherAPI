using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Infrastructure.Persistence.Configurations;

public class WeatherSymbolConfiguration : IEntityTypeConfiguration<WeatherSymbol>
{
    public void Configure(EntityTypeBuilder<WeatherSymbol> builder)
    {
        builder.ToTable("WeatherSymbol");

        builder.HasKey(weatherSymbol => weatherSymbol.Id)
            .HasName("PK_WeatherSymbol");

        builder.HasIndex(weatherSymbol => weatherSymbol.Code)
            .IsUnique()
            .HasDatabaseName("UQ_WeatherSymbol_Code");

        builder.Property(weatherSymbol => weatherSymbol.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(weatherSymbol => weatherSymbol.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(50)")
            .HasMaxLength(50)
            .IsUnicode(false);
    }
}
