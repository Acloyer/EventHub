namespace EventHub.Models.DTOs
{
    public class OrganizerBlacklistDto
    {
        public int Id { get; set; }
        public int OrganizerId { get; set; }
        public int BannedUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Reason { get; set; }
        public string OrganizerName { get; set; } = string.Empty;
        public string BannedUserName { get; set; } = string.Empty;
        public string BannedUserEmail { get; set; } = string.Empty;
    }

    public class CreateBlacklistEntryDto
    {
        public int BannedUserId { get; set; }
        public string? Reason { get; set; }
    }

    public class RemoveBlacklistEntryDto
    {
        public int BannedUserId { get; set; }
    }
} 