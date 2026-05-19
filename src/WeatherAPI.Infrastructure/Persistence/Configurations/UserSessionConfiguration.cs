using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WeatherAPI.Domain.Entities;

namespace WeatherAPI.Infrastructure.Persistence.Configurations;

public class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("UserSession");

        builder.HasKey(session => session.Id)
            .HasName("PK_UserSession");

        builder.Property(session => session.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(session => session.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(session => session.TokenHash)
            .HasColumnName("token_hash")
            .HasColumnType("varchar(64)")
            .HasMaxLength(64)
            .IsUnicode(false)
            .IsRequired();

        builder.HasIndex(session => session.TokenHash)
            .IsUnique()
            .HasDatabaseName("UQ_UserSession_TokenHash");

        builder.Property(session => session.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(session => session.ExpiresAt)
            .HasColumnName("expires_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(session => session.RevokedAt)
            .HasColumnName("revoked_at")
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(session => new { session.UserId, session.ExpiresAt })
            .HasDatabaseName("IX_UserSession_UserId_ExpiresAt");

        builder.HasOne(session => session.User)
            .WithMany()
            .HasForeignKey(session => session.UserId)
            .HasConstraintName("FK_UserSession_AppUser_UserId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
