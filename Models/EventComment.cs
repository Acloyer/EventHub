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
<<<<<<< HEAD
        
        public bool     IsPinned        { get; set; } = false;
        public DateTime? PinnedAt        { get; set; }
=======
<<<<<<< HEAD
        
        public bool     IsPinned        { get; set; } = false;
        public DateTime? PinnedAt        { get; set; }
=======
        public bool     IsPinned        { get; set; }
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
        public int?     ParentCommentId { get; set; }
    }
}
