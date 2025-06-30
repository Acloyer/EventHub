// Models/EventComment.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace EventHub.Models
{
    public class EventComment
    {
        public int      Id         { get; set; }
        public int      EventId    { get; set; }
        public Event    Event      { get; set; }
        public int      UserId     { get; set; }
        public User     User       { get; set; }

        [Required, MaxLength(200)]
        public string   Comment    { get; set; }

        public DateTime PostDate   { get; set; } = DateTime.UtcNow;
        public DateTime? EditDate  { get; set; }

        public bool     IsEdited        { get; set; }
        public bool     IsPinned        { get; set; }
        public int?     ParentCommentId { get; set; }
    }
}
