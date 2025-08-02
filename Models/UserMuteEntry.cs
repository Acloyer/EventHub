using System.ComponentModel.DataAnnotations;

namespace EventHub.Models
{
    public class UserMuteEntry
    {
        [Key] 
        public int UserId { get; set; }
        public User User { get; set; }
        public bool IsMuted { get; set; }
        public DateTime? Until { get; set; }
    }
}
