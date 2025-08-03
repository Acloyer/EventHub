using System.ComponentModel.DataAnnotations;

namespace EventHub.Models.DTOs
{
    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = null!;

        [Required]
        [MinLength(2)]
        [MaxLength(100)]
        public string Name { get; set; } = null!;
    }
}
