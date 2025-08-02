// Models/TelegramVerification.cs
using System;

namespace EventHub.Models
{
    public class TelegramVerification
    {
        public int Id { get; set; }
        public long ChatId { get; set; }
        public int UserId { get; set; }
        public int Code { get; set; }
        public DateTime ExpiresAt { get; set; }
        public User User { get; set; }
    }
}
