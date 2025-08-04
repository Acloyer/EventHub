using System.ComponentModel.DataAnnotations;

namespace EventHub.Models
{
    public class PostReaction
    {
        [Key]
        public int Id { get; set; }
        public int EventId { get; set; }
        public int UserId { get; set; }
        public string Emoji { get; set; } = null!;

        public Event Event { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
