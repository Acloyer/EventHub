namespace EventHub.Models
{
    public class Role
    {
        public int Id { get; set; }
        public required string Name { get; set; }  // "Admin", "Organizer" и т.д.
        public required ICollection<UserRole> UserRoles { get; set; }
    }
}
