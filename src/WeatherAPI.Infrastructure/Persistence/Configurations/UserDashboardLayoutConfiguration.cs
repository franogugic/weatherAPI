using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Infrastructure.Persistence.Configurations;

public class UserDashboardLayoutConfiguration : IEntityTypeConfiguration<UserDashboardLayout>
{
    public void Configure(EntityTypeBuilder<UserDashboardLayout> builder)
    {
        builder.ToTable("UserDashboardLayout");

        builder.HasKey(layout => layout.UserId)
            .HasName("PK_UserDashboardLayout");

        builder.Property(layout => layout.UserId)
            .HasColumnName("user_id")
            .ValueGeneratedNever();

        builder.Property(layout => layout.LayoutJson)
            .HasColumnName("layout_json")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(layout => layout.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasOne(layout => layout.User)
            .WithOne(user => user.DashboardLayout)
            .HasForeignKey<UserDashboardLayout>(layout => layout.UserId)
            .HasConstraintName("FK_UserDashboardLayout_AppUser_UserId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
