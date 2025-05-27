using System.Collections.Generic;

namespace EventHub.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }

        // Навигационное свойство
        public List<UserRole> UserRoles { get; set; }
        public ICollection<FavoriteEvent> FavoriteEvents { get; set; } = new List<FavoriteEvent>();
        public ICollection<PlannedEvent> PlannedEvents { get; set; } = new List<PlannedEvent>();
        public ICollection<Event> CreatedEvents { get; set; } = new List<Event>();
    }
}
