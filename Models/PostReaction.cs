namespace EventHub.Models
{
    public class PostReaction
    {
        public int    Id      { get; set; }
        public int    EventId { get; set; }
        public Event  Event   { get; set; }
        public int    UserId  { get; set; }
        public User   User    { get; set; }
        public string Emoji   { get; set; } = null!;
    }
}
