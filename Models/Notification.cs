using System.ComponentModel.DataAnnotations;

namespace EventHub.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; } = null!;
        public string Message { get; set; } = null!;
        public int EntityId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }

        public User User { get; set; } = null!;
    }
}