using System.ComponentModel.DataAnnotations;

namespace EventHub.Models
{
    public class OrganizerBlacklist
    {
        public int Id { get; set; }
        
        [Required]
        public int OrganizerId { get; set; }
        
        [Required]
        public int BannedUserId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public string? Reason { get; set; }
        
        // Navigation properties
        public User Organizer { get; set; } = null!;
        public User BannedUser { get; set; } = null!;
    }
} 