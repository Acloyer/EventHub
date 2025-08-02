using EventHub.Models;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace EventHub.Models
{
    public class User : IdentityUser<int>
    {
        public string Name { get; set; } = null!;
        public bool IsBanned { get; set; } = false;
<<<<<<< HEAD
        public long? TelegramId { get; set; }
        public int? TelegramCode { get; set; }
=======
<<<<<<< HEAD
        public long? TelegramId { get; set; }
        public int? TelegramCode { get; set; }
=======
<<<<<<< HEAD
        // public long? TelegramId { get; set; }
        public long? TelegramId { get; set; }
        public int? TelegramCode { get; set; }
=======
        public long? TelegramId { get; set; }
        public long? TelegramCode { get; set; }
>>>>>>> 573f3e0705c1e3252b4cddd7cfc9446f4bee2932
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
        public bool IsTelegramVerified { get; set; }
        // public long? TelegramCode { get; set; }
        // public bool IsTelegramVerified { get; set; }
        public bool NotifyBeforeEvent { get; set; } = true;
        public virtual ICollection<FavoriteEvent> FavoriteEvents { get; set; } = new List<FavoriteEvent>();
        public virtual ICollection<PlannedEvent> PlannedEvents { get; set; } = new List<PlannedEvent>();
        public virtual ICollection<Event> CreatedEvents { get; set; } = new List<Event>();
        public virtual ICollection<TelegramVerification> TelegramVerifications { get; set; } = new List<TelegramVerification>();
<<<<<<< HEAD
        public ICollection<PostReaction> PostReactions { get; set; } = new List<PostReaction>();
        public ICollection<EventComment> EventComments { get; set; } = new List<EventComment>();
=======
<<<<<<< HEAD
        public ICollection<PostReaction> PostReactions { get; set; } = new List<PostReaction>();
        public ICollection<EventComment> EventComments { get; set; } = new List<EventComment>();
=======
<<<<<<< HEAD
        public ICollection<PostReaction> PostReactions { get; set; } = new List<PostReaction>();
        public ICollection<EventComment> EventComments { get; set; } = new List<EventComment>();
=======

>>>>>>> 573f3e0705c1e3252b4cddd7cfc9446f4bee2932
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
    }
}