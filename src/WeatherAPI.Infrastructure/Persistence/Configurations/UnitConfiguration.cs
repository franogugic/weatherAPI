using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Infrastructure.Persistence.Configurations;

public class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        builder.ToTable("Unit");

        builder.HasKey(unit => unit.Id)
            .HasName("PK_Unit");

        builder.HasIndex(unit => unit.Value)
            .IsUnique()
            .HasDatabaseName("UQ_Unit_Value");

        builder.Property(unit => unit.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(unit => unit.Value)
            .HasColumnName("value")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .IsUnicode(false);

        builder.Property(unit => unit.DisplayName)
            .HasColumnName("display_name")
            .HasColumnType("varchar(20)")
            .HasMaxLength(20)
            .IsUnicode(false);

        builder.Property(unit => unit.Description)
            .HasColumnName("desc")
            .HasColumnType("varchar(50)")
            .HasMaxLength(50)
            .IsUnicode(false);
    }
}
