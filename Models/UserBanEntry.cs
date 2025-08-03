using System.ComponentModel.DataAnnotations;

namespace EventHub.Models
{
    public class UserBanEntry
    {
        [Key] 
        public int UserId { get; set; }
        public User User { get; set; }
        public bool IsBanned { get; set; }
        public DateTime? Until { get; set; }
        public string? Reason { get; set; }
        public string? BannedBy { get; set; }
    }
} 