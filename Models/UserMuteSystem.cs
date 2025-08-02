using System.ComponentModel.DataAnnotations;

namespace EventHub.Models
{
    public class UserMuteSystem
    {
        [Key]
        public int      UserId   { get; set; }
        public bool     IsMuted  { get; set; }
        public DateTime? MuteUntil { get; set; }

        public User     User     { get; set; }
    }
}