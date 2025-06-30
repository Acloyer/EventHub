namespace EventHub.Models
{
    public class Notification
    {
        public int      Id        { get; set; }
        public int      UserId    { get; set; }
        public string   Type      { get; set; }    // e.g. "Reply", "Reaction"
        public int      EntityId  { get; set; }    // ID коммента или события
        public DateTime CreatedAt { get; set; }
        public bool     IsRead    { get; set; }
        
        // Навигация:
        public User     User      { get; set; }    
    }
}