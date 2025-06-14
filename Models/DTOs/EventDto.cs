namespace EventHub.Models.DTOs
{
    public class EventDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int MaxParticipants { get; set; }
        public bool IsFavorite { get; set; }
        public bool IsPlanned { get; set; }
        public string? OrganizerEmail { get; set; }
        public int CreatorId { get; set; }
    }
} 