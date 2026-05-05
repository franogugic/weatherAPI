using WeatherAPI.Domain.Enums;

namespace WeatherAPI.Domain.Entities;

public class User
{
    private User()
    {
    }

    private User(string firstName, string lastName, string email, string passwordHash, UserRole role)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        CreatedAt = DateTime.UtcNow;
    }
    
    public int Id { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; } = UserRole.User;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
    
    //public UserPreference? Preference { get; private set; }
    //public ICollection<UserFavoriteLocation> FavoriteLocations { get; private set; } = new List<UserFavoriteLocation>();

    public static User Create(
        string firstName,
        string lastName,
        string email,
        string passwordHash,
        UserRole role = UserRole.User)
    {
        return new User(firstName, lastName, email, passwordHash, role);
    }
}
