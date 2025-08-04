// Models/EventComment.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace EventHub.Models
{
    public class EventComment
    {
        [Key]
        public int Id { get; set; }
        public int EventId { get; set; }
        public int UserId { get; set; }
        public string Comment { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime PostDate { get; set; } = DateTime.UtcNow;
        public DateTime? EditDate { get; set; }
        public bool IsPinned { get; set; }
        public DateTime? PinnedAt { get; set; }
        public bool IsEdited { get; set; }
        public int? ParentCommentId { get; set; }

        public Event Event { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
