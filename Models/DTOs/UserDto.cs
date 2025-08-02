namespace EventHub.Models.DTOs
{
    public class UserDto
    {
        public UserDto() { }

        public UserDto(User u)
        {
            Id                 = u.Id;
            Email              = u.Email;
            Name               = u.Name;
            Roles              = new List<string>();
            TelegramId         = u.TelegramId;
            IsTelegramVerified = u.IsTelegramVerified;
            NotifyBeforeEvent  = u.NotifyBeforeEvent;
            IsBanned           = u.IsBanned;
        }

        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
        public List<string> Roles { get; set; } = new();
        public long? TelegramId { get; set; }
        public bool IsTelegramVerified { get; set; }
        public bool NotifyBeforeEvent { get; set; }
        public bool IsBanned { get; set; }
    }
<<<<<<< HEAD

    public class AssignRolesDto
    {
        public List<string> Roles { get; set; } = new();
    }
<<<<<<< HEAD
=======
=======
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
}
