using System.ComponentModel.DataAnnotations;

namespace EventHub.Models.DTOs
{
    public class UserUpdateDto
    {
        [EmailAddress]
        public string? Email { get; set; }

        [MinLength(2)]
        [MaxLength(100)]
        public string? Name { get; set; }

        public int? TelegramId { get; set; }
        public bool IsTelegramVerified { get; set; }
        public bool NotifyBeforeEvent { get; set; }
        public bool IsBanned { get; set; }
    }
} 