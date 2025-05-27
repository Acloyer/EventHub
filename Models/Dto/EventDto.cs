namespace EventHub.Models.Dto
{
    public class EventDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
