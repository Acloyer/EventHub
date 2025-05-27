using EventHub.Models;
using System.Collections.Generic;

namespace EventHub.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? TelegramId { get; set; }
        public bool NotifyBeforeEvent { get; set; } = true;
        public List<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public List<FavoriteEvent> FavoriteEvents { get; set; } = new List<FavoriteEvent>();
        public List<PlannedEvent> PlannedEvents { get; set; } = new List<PlannedEvent>();
        public List<Event> CreatedEvents { get; set; } = new List<Event>();
    }
}