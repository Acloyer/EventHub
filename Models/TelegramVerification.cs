// Models/TelegramVerification.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace EventHub.Models
{
    public class TelegramVerification
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public long TelegramId { get; set; }
        public int Code { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsVerified { get; set; }

        public User User { get; set; } = null!;
    }
}
