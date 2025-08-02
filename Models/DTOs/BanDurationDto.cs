namespace EventHub.Models.DTOs
{
    public class BanDurationDto
    {
        public int Seconds { get; set; }
        public int Minutes { get; set; }
        public int Hours { get; set; }
        public string? Reason { get; set; }
    }
} 