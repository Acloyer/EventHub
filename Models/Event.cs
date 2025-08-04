using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventHub.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int MaxParticipants { get; set; }
        public int CreatorId { get; set; }
        public virtual User? Creator { get; set; }
        public virtual ICollection<FavoriteEvent> FavoriteEvents { get; set; } = new List<FavoriteEvent>();
        public virtual ICollection<PlannedEvent> PlannedEvents { get; set; } = new List<PlannedEvent>();
        [NotMapped]
        public bool IsFavorite { get; set; }

        [NotMapped]
        public bool IsPlanned { get; set; }

        public ICollection<PostReaction> PostReactions { get; set; } = new List<PostReaction>();
        public ICollection<EventComment> EventComments { get; set; } = new List<EventComment>();
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
