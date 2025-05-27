namespace EventHub.Models;

public class FavoriteEvent
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int EventId { get; set; }

    public User User { get; set; } = null!;
    public Event Event { get; set; } = null!;
}
