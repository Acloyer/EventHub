using System.ComponentModel.DataAnnotations;
public class ProfileUpdateDto
{
    [EmailAddress]
    public string? Email { get; set; }

    [MinLength(2), MaxLength(100)]
    public string? Name { get; set; }

    public long? TelegramId { get; set; }
    public bool NotifyBeforeEvent { get; set; }
}