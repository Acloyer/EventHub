﻿using System;
using System.Collections.Generic;

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
        public bool IsFavorite { get; set; }
        public bool IsPlanned { get; set; }
    }
}
