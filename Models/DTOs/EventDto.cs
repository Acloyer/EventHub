// File: Models/DTOs/EventDto.cs
using System;
using EventHub.Models;

namespace EventHub.Models.DTOs
{
    public class EventDto
    {
        public EventDto() { }

        public EventDto(Event e)
        {
            Id              = e.Id;
            Title           = e.Title;
            Description     = e.Description;
            StartDate       = e.StartDate;
            EndDate         = e.EndDate;
            Category        = e.Category;
            Location        = e.Location!;
            MaxParticipants = e.MaxParticipants;
            CreatorId       = e.CreatorId;
            OrganizerEmail  = e.Creator!.Email!;
            OrganizerName   = e.Creator!.Name!;
            IsFavorite      = false;
            IsPlanned       = false;
        }

        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Category { get; set; } = null!;
        public string Location { get; set; } = null!;
        public int MaxParticipants { get; set; }
        public bool IsFavorite { get; set; }
        public bool IsPlanned { get; set; }
        public string OrganizerEmail { get; set; } = null!;
        public string OrganizerName { get; set; } = null!;
        public int CreatorId { get; set; }
    }
}