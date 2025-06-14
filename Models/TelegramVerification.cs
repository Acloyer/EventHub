using System;

namespace EventHub.Models
{
    public class TelegramVerification
    {
        public int Id { get; set; }
        public required string ChatId { get; set; }
        public required string VerificationCode { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 