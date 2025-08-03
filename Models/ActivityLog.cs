namespace EventHub.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public int? EntityId { get; set; }
        public string Details { get; set; } = string.Empty;
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
        public string IpAddress { get; set; } = string.Empty;
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
        public string UserAgent { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        
        // Navigation properties
        public User User { get; set; } = null!;
    }
} 