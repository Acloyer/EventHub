using System;

namespace EventHub.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public int OrganizerId { get; set; }
        public User Organizer { get; set; }
    }
}
