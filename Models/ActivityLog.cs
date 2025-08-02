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
        public string IpAddress { get; set; } = string.Empty;
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
        public string UserAgent { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        
        // Navigation properties
        public User User { get; set; } = null!;
    }
} 