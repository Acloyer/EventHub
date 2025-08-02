namespace EventHub.Models
{
    public class Notification
    {
        public int      Id        { get; set; }
        public int      UserId    { get; set; }
        public string   Type      { get; set; }    // e.g. "Reply", "Reaction"
<<<<<<< HEAD
        public int      EntityId  { get; set; }    // ID of comment or event
        public DateTime CreatedAt { get; set; }
        public bool     IsRead    { get; set; }
        
        // Navigation:
=======
        public int      EntityId  { get; set; }    // ID коммента или события
        public DateTime CreatedAt { get; set; }
        public bool     IsRead    { get; set; }
        
        // Навигация:
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
        public User     User      { get; set; }    
    }
}