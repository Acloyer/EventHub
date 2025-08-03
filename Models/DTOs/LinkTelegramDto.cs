using System.ComponentModel.DataAnnotations;
namespace EventHub.Models.DTOs
{
    public class LinkTelegramDto
    {
        [Required]
        public string Username { get; set; } = null!;
    }
}