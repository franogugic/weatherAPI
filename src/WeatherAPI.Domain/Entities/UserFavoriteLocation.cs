namespace WeatherAPI.Domain.Entities;

public class UserFavoriteLocation
{
    private UserFavoriteLocation()
    {
    }

    private UserFavoriteLocation(int userId, short locationId)
    {
        UserId = userId;
        LocationId = locationId;
        CreatedAt = DateTime.UtcNow;
    }

    public int UserId { get; private set; }
    public short LocationId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public User User { get; private set; } = null!;
    public Location Location { get; private set; } = null!;

    public static UserFavoriteLocation Create(int userId, short locationId)
    {
        return new UserFavoriteLocation(userId, locationId);
    }
}
