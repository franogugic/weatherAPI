using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WeatherAPI.Domain.Entities;
using WeatherAPI.Domain.Enums;

namespace WeatherAPI.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("AppUser");

        builder.HasKey(user => user.Id)
            .HasName("PK_AppUser");

        builder.Property(user => user.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();
        
        builder.Property(user => user.FirstName)
            .HasColumnName("first_name")
            .HasColumnType("varchar(50)")
            .HasMaxLength(50)
            .IsUnicode(false)
            .IsRequired();
        
        builder.Property(user => user.LastName)
            .HasColumnName("last_name")
            .HasColumnType("varchar(50)")
            .HasMaxLength(50)
            .IsUnicode(false)
            .IsRequired();
        
        builder.Property(user => user.Email)
            .HasColumnName("email")
            .HasColumnType("varchar(100)")
            .HasMaxLength(100)
            .IsUnicode(false)
            .IsRequired();

        builder.HasIndex(user => user.Email)
            .IsUnique()
            .HasDatabaseName("UQ_AppUser_Email");
        
        builder.Property(user => user.PasswordHash)
            .HasColumnName("password_hash")
            .HasColumnType("varchar(500)")
            .HasMaxLength(500)
            .IsUnicode(false)
            .IsRequired();
        
        builder.Property(user => user.Role)
            .HasColumnName("role")
            .HasConversion<string>()
            .HasColumnType("varchar(20)")
            .HasDefaultValue(UserRole.User)
            .HasMaxLength(20)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(user => user.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("datetime2(0)")
            .IsRequired();
        
        builder.Property(user => user.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("datetime2(0)")
            .IsRequired();
            
    }
}
