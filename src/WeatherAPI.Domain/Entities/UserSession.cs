namespace WeatherAPI.Domain.Entities;

public class UserSession
{
    private UserSession() {}

    private UserSession(User user, string tokenHash, DateTime expiresAt)
    {
        var now = DateTime.UtcNow;

        User = user;
        UserId = user.Id;
        TokenHash = tokenHash;
        CreatedAt = now;
        ExpiresAt = expiresAt;
    }

    public int Id { get; private set; }
    public int UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public User User { get; private set; } = null!;

    public static UserSession Create(User user, string tokenHash, DateTime expiresAt)
    {
        return new UserSession(user, tokenHash, expiresAt);
    }

    public void Revoke()
    {
        RevokedAt = DateTime.UtcNow;
    }
    
    public bool IsActive()
    {
        var now = DateTime.UtcNow;
        return RevokedAt is null && ExpiresAt > now;
    }
}
