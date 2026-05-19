using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Infrastructure.Persistence.Configurations;

public class UserFavoriteLocationConfiguration : IEntityTypeConfiguration<UserFavoriteLocation>
{
    public void Configure(EntityTypeBuilder<UserFavoriteLocation> builder)
    {
        builder.ToTable("UserFavoriteLocation");

        builder.HasKey(favoriteLocation => new
            {
                favoriteLocation.UserId,
                favoriteLocation.LocationId
            })
            .HasName("PK_UserFavoriteLocation");

        builder.Property(favoriteLocation => favoriteLocation.UserId)
            .HasColumnName("user_id")
            .ValueGeneratedNever();

        builder.Property(favoriteLocation => favoriteLocation.LocationId)
            .HasColumnName("location_id")
            .ValueGeneratedNever();

        builder.Property(favoriteLocation => favoriteLocation.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasOne(favoriteLocation => favoriteLocation.User)
            .WithMany(user => user.FavoriteLocations)
            .HasForeignKey(favoriteLocation => favoriteLocation.UserId)
            .HasConstraintName("FK_UserFavoriteLocation_AppUser_UserId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(favoriteLocation => favoriteLocation.Location);
    }
}
