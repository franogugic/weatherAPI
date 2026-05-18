using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Infrastructure.Persistence.Configurations;

public class UserPreferenceConfiguration : IEntityTypeConfiguration<UserPreference>
{
    public void Configure(EntityTypeBuilder<UserPreference> builder)
    {
        builder.ToTable("UserPreference");

        builder.HasKey(preference => preference.UserId)
            .HasName("PK_UserPreference");

        builder.Property(preference => preference.UserId)
            .HasColumnName("user_id")
            .ValueGeneratedNever();

        builder.Property(preference => preference.TemperatureUnit)
            .HasColumnName("temperature_unit")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(preference => preference.WindSpeedUnit)
            .HasColumnName("wind_speed_unit")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(preference => preference.PressureUnit)
            .HasColumnName("pressure_unit")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(preference => preference.CloudinessUnit)
            .HasColumnName("cloudiness_unit")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(preference => preference.PrecipitationUnit)
            .HasColumnName("precipitation_unit")
            .HasColumnType("varchar(30)")
            .HasMaxLength(30)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(preference => preference.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasOne(preference => preference.User)
            .WithOne(user => user.Preference)
            .HasForeignKey<UserPreference>(preference => preference.UserId)
            .HasConstraintName("FK_UserPreference_AppUser_UserId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
