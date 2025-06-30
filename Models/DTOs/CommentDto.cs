using System.ComponentModel.DataAnnotations;

namespace EventHub.Models.DTOs
{
    public class CommentDto
    {
        public int      Id       { get; set; }
        public int      UserId   { get; set; }
        [Required, MaxLength(200)]
        public string Comment { get; set; } = null!;
        public DateTime PostDate { get; set; }
        public bool     IsEdited { get; set; }
        public DateTime? EditDate{ get; set; }
        public bool     IsPinned { get; set; }
    }
    
    public class CreateCommentDto
    {
        [Required, MaxLength(200)]
        public string Comment { get; set; } = null!;
    }

    public class UpdateCommentDto
    {
        [Required, MaxLength(200)]
        public string Comment { get; set; } = null!;
    }
}
