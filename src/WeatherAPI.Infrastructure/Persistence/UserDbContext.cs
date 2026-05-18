using Microsoft.EntityFrameworkCore;
using WeatherAPI.Domain.Entities;
using WeatherAPI.Infrastructure.Persistence.Configurations;

namespace WeatherAPI.Infrastructure.Persistence;

public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
    public DbSet<UserFavoriteLocation> UserFavoriteLocations => Set<UserFavoriteLocation>();
    public DbSet<UserDashboardLayout> UserDashboardLayouts => Set<UserDashboardLayout>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new UserSessionConfiguration());
        modelBuilder.ApplyConfiguration(new UserPreferenceConfiguration());
        modelBuilder.ApplyConfiguration(new UserFavoriteLocationConfiguration());
        modelBuilder.ApplyConfiguration(new UserDashboardLayoutConfiguration());
    }
}
