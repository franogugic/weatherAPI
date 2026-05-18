namespace WeatherAPI.Domain.Entities;

public class UserDashboardLayout
{
    private UserDashboardLayout()
    {
    }

    private UserDashboardLayout(int userId, string layoutJson)
    {
        UserId = userId;
        LayoutJson = layoutJson;
        UpdatedAt = DateTime.UtcNow;
    }

    public int UserId { get; private set; }
    public string LayoutJson { get; private set; } = string.Empty;
    public DateTime UpdatedAt { get; private set; }

    public User User { get; private set; } = null!;

    public static UserDashboardLayout Create(int userId, string layoutJson)
    {
        return new UserDashboardLayout(userId, layoutJson);
    }

    public void Update(string layoutJson)
    {
        LayoutJson = layoutJson;
        UpdatedAt = DateTime.UtcNow;
    }
}
