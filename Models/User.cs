using System.Collections.Generic;

namespace EventHub.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }

        // Навигационное свойство
        public List<UserRole> UserRoles { get; set; }
    }
}
