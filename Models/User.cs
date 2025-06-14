using EventHub.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EventHub.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        
        [JsonIgnore]
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
        
        [JsonIgnore]
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();

        public bool IsBanned { get; set; } = false;

        public string? TelegramId { get; set; }
        public string? TelegramCode { get; set; }

        public bool IsTelegramVerified { get; set; }
        public bool NotifyBeforeEvent { get; set; } = true;
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<FavoriteEvent> FavoriteEvents { get; set; } = new List<FavoriteEvent>();
        public virtual ICollection<PlannedEvent> PlannedEvents { get; set; } = new List<PlannedEvent>();
        public virtual ICollection<Event> CreatedEvents { get; set; } = new List<Event>();
    }
}