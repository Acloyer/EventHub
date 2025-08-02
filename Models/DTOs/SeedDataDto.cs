namespace EventHub.Models.DTOs
{
    public class SeedDataDto
    {
        public int OwnerCount { get; set; } = 1; // Always 1, cannot be changed
        public int SeniorAdminCount { get; set; } = 1;
        public int AdminCount { get; set; } = 2;
        public int OrganizerCount { get; set; } = 1;
        public int RegularUserCount { get; set; } = 10;
        public int PastEventCount { get; set; } = 10;
        public int FutureEventCount { get; set; } = 20;
        public int PositiveCommentCount { get; set; } = 24; // 40% of 60
        public int NeutralCommentCount { get; set; } = 24; // 40% of 60
        public int NegativeCommentCount { get; set; } = 12; // 20% of 60
        public bool CreateReactions { get; set; } = true;
        public bool CreateFavorites { get; set; } = true;
        public bool CreatePlannedEvents { get; set; } = true;
    }
} 