namespace EventHub.Models.DTOs
{
    public class RoleDto
    {
        public int    Id        { get; set; }
        public string Name      { get; set; } = null!;
        public int    UserCount { get; set; }
    }
}
