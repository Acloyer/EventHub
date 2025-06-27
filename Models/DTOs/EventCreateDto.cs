// File: Models/DTOs/EventDto.cs
using System;
using EventHub.Models;

namespace EventHub.Models.DTOs
{
    public class EventCreateDto
    {
        public EventCreateDto() { }

        public EventCreateDto(Event e)
        {
            Title           = e.Title;
            Description     = e.Description;
            StartDate       = e.StartDate;
            EndDate         = e.EndDate;
            Category        = e.Category;
            Location        = e.Location!;
            MaxParticipants = e.MaxParticipants;
        }

        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Category { get; set; } = null!;
        public string Location { get; set; } = null!;
        public int MaxParticipants { get; set; }
    }
}