namespace EventHub.Models
{
    public class PostReaction
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public bool IsLike { get; set; }
    }
}
