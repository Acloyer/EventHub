namespace EventHub.Models.DTOs
{
    public class UserUpdateDto
    {
        public string? TelegramId { get; set; }
        public bool IsTelegramVerified { get; set; }
        public bool NotifyBeforeEvent { get; set; }
    }
} 