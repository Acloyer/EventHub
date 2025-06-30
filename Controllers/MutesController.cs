// Controllers/MutesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using EventHub.Data;
using EventHub.Models.DTOs;
using EventHub.Models;
using System.Security.Claims; 
using Microsoft.AspNetCore.Identity;

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
            var entry = await _db.UserMuteEntries.FindAsync(userId);
            if (entry == null)
            {
                entry = new UserMuteEntry { UserId = userId, IsMuted = dto.IsMuted };
                _db.UserMuteEntries.Add(entry);
            }
            else
            {
                entry.IsMuted = dto.IsMuted;
            }
            await _db.SaveChangesAsync();
            if(dto.IsMuted == false){
                return Ok(new { succeeded = true, message = "Пользователь размучен." });
            }
            else{
                return Ok(new { succeeded = true, message = "Пользователь замучен." });
            }
        }

        [HttpPost("duration"), Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> SetMuteDuration(int userId, [FromBody] MuteDurationDto dto)
        {
            var entry = await _db.UserMuteEntries.FindAsync(userId);
            if (entry == null)
            {
                entry = new UserMuteEntry { UserId = userId };
                _db.UserMuteEntries.Add(entry);
            }

            entry.IsMuted = true;
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

            return Ok(new
            {
                succeeded = true,
                message   = $"Пользователь «{name}» замучен на {period}."
            });
        }

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
}
