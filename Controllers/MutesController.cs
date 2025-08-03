// Controllers/MutesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using EventHub.Data;
using EventHub.Models.DTOs;
using EventHub.Models;
using System.Security.Claims; 
using Microsoft.AspNetCore.Identity;
<<<<<<< HEAD
using Microsoft.EntityFrameworkCore;
=======
<<<<<<< HEAD
using Microsoft.EntityFrameworkCore;
=======
<<<<<<< HEAD
using Microsoft.EntityFrameworkCore;
=======
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9

namespace EventHub.Controllers
{
    [ApiController]
    [Route("api/users/{userId}/mute")]
    public class MutesController : ControllerBase
    {
        readonly EventHubDbContext _db;
        private readonly UserManager<User> _users;
        public MutesController(EventHubDbContext db, UserManager<User> users)
        {
            _db = db;
            _users = users;
        }

        [HttpPost, Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> SetMute(int userId, [FromBody] MuteDto dto)
        {
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var currentUser = await _users.FindByIdAsync(currentUserId.ToString());
            var targetUser = await _users.FindByIdAsync(userId.ToString());

            if (currentUser == null || targetUser == null)
            {
                return NotFound("User not found");
            }

            // Проверяем ранги
            var currentUserRoles = await _users.GetRolesAsync(currentUser);
            var targetUserRoles = await _users.GetRolesAsync(targetUser);

            var currentUserLevel = GetRoleLevel(currentUserRoles);
            var targetUserLevel = GetRoleLevel(targetUserRoles);

            // Проверяем, может ли текущий пользователь мутить целевого
            if (currentUserLevel <= targetUserLevel)
            {
                return Forbid("You cannot mute a user with equal or higher rank");
            }

<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
=======
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
            var entry = await _db.UserMuteEntries.FindAsync(userId);
            if (entry == null)
            {
                entry = new UserMuteEntry { UserId = userId, IsMuted = dto.IsMuted };
                _db.UserMuteEntries.Add(entry);
            }
            else
            {
                entry.IsMuted = dto.IsMuted;
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
                if (!dto.IsMuted)
                {
                    entry.Until = null; // Reset mute time when unmuting
                }
            }
            await _db.SaveChangesAsync();
            
            if(dto.IsMuted == false){
                return Ok(new { succeeded = true, message = "User unmuted." });
            }
            else{
                return Ok(new { succeeded = true, message = "User muted." });
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
=======
            }
            await _db.SaveChangesAsync();
            if(dto.IsMuted == false){
                return Ok(new { succeeded = true, message = "Пользователь размучен." });
            }
            else{
                return Ok(new { succeeded = true, message = "Пользователь замучен." });
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
            }
        }

        [HttpPost("duration"), Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> SetMuteDuration(int userId, [FromBody] MuteDurationDto dto)
        {
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var currentUser = await _users.FindByIdAsync(currentUserId.ToString());
            var targetUser = await _users.FindByIdAsync(userId.ToString());

            if (currentUser == null || targetUser == null)
            {
                return NotFound("User not found");
            }

            // Проверяем ранги
            var currentUserRoles = await _users.GetRolesAsync(currentUser);
            var targetUserRoles = await _users.GetRolesAsync(targetUser);

            var currentUserLevel = GetRoleLevel(currentUserRoles);
            var targetUserLevel = GetRoleLevel(targetUserRoles);

            // Проверяем, может ли текущий пользователь мутить целевого
            if (currentUserLevel <= targetUserLevel)
            {
                return Forbid("You cannot mute a user with equal or higher rank");
            }

<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
=======
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
            var entry = await _db.UserMuteEntries.FindAsync(userId);
            if (entry == null)
            {
                entry = new UserMuteEntry { UserId = userId };
                _db.UserMuteEntries.Add(entry);
            }

            entry.IsMuted = true;
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
            var totalSeconds = dto.Seconds + (dto.Minutes * 60) + (dto.Hours * 3600);
            entry.Until = DateTime.UtcNow.AddSeconds(totalSeconds);

            await _db.SaveChangesAsync();

            var totalMinutes = totalSeconds / 60;
            var hours = totalMinutes / 60;
            var minutes = totalMinutes % 60;
            var seconds = totalSeconds % 60;
            
            string period;
            if (hours > 0 && minutes > 0 && seconds > 0)
                period = $"{hours} час{(hours>1?"ов":hours==1?"":"")} {minutes} минут{(minutes>1?"":minutes==1?"":"")} {seconds} секунд{(seconds>1?"":seconds==1?"а":"")}";
            else if (hours > 0 && minutes > 0)
                period = $"{hours} час{(hours>1?"ов":hours==1?"":"")} {minutes} минут{(minutes>1?"":minutes==1?"":"")}";
            else if (hours > 0 && seconds > 0)
                period = $"{hours} час{(hours>1?"ов":hours==1?"":"")} {seconds} секунд{(seconds>1?"":seconds==1?"а":"")}";
            else if (hours > 0)
                period = $"{hours} час{(hours>1?"ов":hours==1?"":"")}";
            else if (minutes > 0 && seconds > 0)
                period = $"{minutes} минут{(minutes>1?"ы":minutes==1?"а":"")} {seconds} секунд{(seconds>1?"":seconds==1?"а":"")}";
            else if (minutes > 0)
                period = $"{minutes} минут{(minutes>1?"ы":minutes==1?"а":"")}";
            else
                period = $"{seconds} секунд{(seconds>1?"":seconds==1?"а":"")}";

            var name = targetUser?.Name ?? targetUser?.Email ?? userId.ToString();
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
=======
            entry.Until = DateTime.UtcNow.AddMinutes(dto.Minutes);

            await _db.SaveChangesAsync();

            var hours   = dto.Minutes / 60;
            var minutes = dto.Minutes % 60;
            string period;
            if (hours > 0 && minutes > 0)
                period = $"{hours} час{(hours>1?"ов":hours==1?"":"")} {minutes} минут{(minutes>1?"":minutes==1?"":"")}";
            else if (hours > 0)
                period = $"{hours} час{(hours>1?"ов":hours==1?"":"")}";
            else
                period = $"{minutes} минут{(minutes>1?"":minutes==1?"":"")}";

            var user = await _users.FindByIdAsync(userId.ToString());
            var name = user?.Name ?? user?.Email ?? userId.ToString();
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9

            return Ok(new
            {
                succeeded = true,
                message   = $"Пользователь «{name}» замучен на {period}."
            });
        }

<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
        private int GetRoleLevel(IList<string> roles)
        {
            if (roles.Contains("Owner")) return 4;
            if (roles.Contains("SeniorAdmin")) return 3;
            if (roles.Contains("Admin")) return 2;
            if (roles.Contains("Organizer")) return 1;
            return 0; // User
        }

<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
=======
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
        private async Task<UserMuteEntry?> RefreshIfExpired(UserMuteEntry? entry)
        {
            if (entry != null && entry.IsMuted && entry.Until.HasValue)
            {
                if (entry.Until.Value <= DateTime.UtcNow)
                {
                    entry.IsMuted = false;
                    entry.Until   = null;
                    _db.UserMuteEntries.Update(entry);
                    await _db.SaveChangesAsync();
                }
            }
            return entry;
        }

        // GET /api/users/{userId}/mute  — статус конкретного юзера (Admin+)
        [HttpGet, Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> GetMuteStatus(int userId)
        {
            var entry = await _db.UserMuteEntries.FindAsync(userId);
            entry = await RefreshIfExpired(entry);

            return Ok(new
            {
                isMuted = entry?.IsMuted ?? false,
                until   = entry?.Until
            });
        }


        // // GET /api/users/me/mute  — свой статус
        // GET /api/users/status — свой статус
        // [HttpGet("~/api/users/me/mute"), Authorize]
        [HttpGet("~/api/users/status"), Authorize]
        public async Task<IActionResult> MyMuteStatus()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var entry  = await _db.UserMuteEntries.FindAsync(userId);
            entry = await RefreshIfExpired(entry);

            return Ok(new
            {
                isMuted = entry?.IsMuted ?? false,
                until   = entry?.Until
            });
        }
    }
<<<<<<< HEAD
    
=======
<<<<<<< HEAD
    
=======
<<<<<<< HEAD
    
=======
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
}
