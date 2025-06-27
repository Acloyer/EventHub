using EventHub.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace EventHub.Models
{
    public class User : IdentityUser<int>
    {
        public string Name { get; set; } = null!;
        public bool IsBanned { get; set; } = false;
        public long? TelegramId { get; set; }
        public long? TelegramCode { get; set; }
        public bool IsTelegramVerified { get; set; }
        public bool NotifyBeforeEvent { get; set; } = true;
        public virtual ICollection<FavoriteEvent> FavoriteEvents { get; set; } = new List<FavoriteEvent>();
        public virtual ICollection<PlannedEvent> PlannedEvents { get; set; } = new List<PlannedEvent>();
        public virtual ICollection<Event> CreatedEvents { get; set; } = new List<Event>();
        public virtual ICollection<TelegramVerification> TelegramVerifications { get; set; } = new List<TelegramVerification>();

    }
}