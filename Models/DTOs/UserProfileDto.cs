namespace EventHub.Models.DTOs
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string[] Roles { get; set; } = null!;
        public long? TelegramId { get; set; }
        public bool IsTelegramVerified { get; set; }
        public bool NotifyBeforeEvent { get; set; }

        // ← Новое
        public bool IsBanned { get; set; }
        // public EventDto[] CreatedEvents { get; set; } = Array.Empty<EventDto>();
        // public EventDto[] FavoriteEvents { get; set; } = Array.Empty<EventDto>();
        // public EventDto[] PlannedEvents { get; set; } = Array.Empty<EventDto>();
    }
}
