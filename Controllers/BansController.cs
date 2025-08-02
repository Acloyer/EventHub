using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using EventHub.Data;
using EventHub.Models;
using EventHub.Models.DTOs;
using EventHub.Services;

namespace EventHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
    public class BansController : ControllerBase
    {
        private readonly EventHubDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger<BansController> _logger;

        private static readonly Dictionary<string, int> RoleHierarchy = new()
        {
            ["User"] = 0,
            ["Organizer"] = 1,
            ["Admin"] = 2,
            ["SeniorAdmin"] = 3,
            ["Owner"] = 4
        };

        public BansController(
            EventHubDbContext db,
            UserManager<User> userManager,
            IActivityLogService activityLogService,
            ILogger<BansController> logger)
        {
            _db = db;
            _userManager = userManager;
            _activityLogService = activityLogService;
            _logger = logger;
        }

        [HttpPost("{userId:int}")]
        public async Task<IActionResult> SetBan(int userId, [FromBody] BanDto dto)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var currentUser = await _userManager.FindByIdAsync(currentUserId.ToString());
            var targetUser = await _userManager.FindByIdAsync(userId.ToString());

            if (currentUser == null || targetUser == null)
            {
                return NotFound("User not found");
            }

            // Проверяем ранги
            var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
            var targetUserRoles = await _userManager.GetRolesAsync(targetUser);

            var currentUserLevel = GetRoleLevel(currentUserRoles);
            var targetUserLevel = GetRoleLevel(targetUserRoles);

            // Проверяем, может ли текущий пользователь банить целевого
            if (targetUserLevel >= currentUserLevel)
            {
                return Forbid("Cannot ban user with equal or higher role");
            }

            var entry = await _db.UserBanEntries.FindAsync(userId);
            if (entry == null)
            {
                entry = new UserBanEntry { 
                    UserId = userId, 
                    IsBanned = dto.IsBanned,
                    Reason = dto.Reason,
                    BannedBy = currentUser.Name
                };
                _db.UserBanEntries.Add(entry);
            }
            else
            {
                entry.IsBanned = dto.IsBanned;
                if (dto.IsBanned)
                {
                    entry.Reason = dto.Reason;
                    entry.BannedBy = currentUser.Name;
                }
                else
                {
                    entry.Until = null; // Сбрасываем время бана при разбане
                    entry.Reason = null;
                    entry.BannedBy = null;
                }
            }
            await _db.SaveChangesAsync();
            
            // Логируем действие
            await _activityLogService.LogUserActivityAsync(
                currentUserId,
                dto.IsBanned ? "USER_BANNED" : "USER_UNBANNED",
                $"User {targetUser.Name} ({targetUser.Email}) was {(dto.IsBanned ? "banned" : "unbanned")} by {currentUser.Name}",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                HttpContext.Request.Headers["User-Agent"].ToString()
            );
            
            if(dto.IsBanned == false){
                return Ok(new { succeeded = true, message = "Пользователь разбанен." });
            }
            else{
                return Ok(new { succeeded = true, message = "Пользователь забанен." });
            }
        }

        [HttpPost("{userId:int}/duration")]
        public async Task<IActionResult> SetBanDuration(int userId, [FromBody] BanDurationDto dto)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var currentUser = await _userManager.FindByIdAsync(currentUserId.ToString());
            var targetUser = await _userManager.FindByIdAsync(userId.ToString());

            if (currentUser == null || targetUser == null)
            {
                return NotFound("User not found");
            }

            // Проверяем ранги
            var currentUserRoles = await _userManager.GetRolesAsync(currentUser);
            var targetUserRoles = await _userManager.GetRolesAsync(targetUser);

            var currentUserLevel = GetRoleLevel(currentUserRoles);
            var targetUserLevel = GetRoleLevel(targetUserRoles);

            // Проверяем, может ли текущий пользователь банить целевого
            if (targetUserLevel >= currentUserLevel)
            {
                return Forbid("Cannot ban user with equal or higher role");
            }

            var totalSeconds = dto.Seconds + (dto.Minutes * 60) + (dto.Hours * 3600);
            var banUntil = DateTime.UtcNow.AddSeconds(totalSeconds);

            var entry = await _db.UserBanEntries.FindAsync(userId);
            if (entry == null)
            {
                entry = new UserBanEntry 
                { 
                    UserId = userId, 
                    IsBanned = true,
                    Until = banUntil,
                    Reason = dto.Reason,
                    BannedBy = currentUser.Name
                };
                _db.UserBanEntries.Add(entry);
            }
            else
            {
                entry.IsBanned = true;
                entry.Until = banUntil;
                entry.Reason = dto.Reason;
                entry.BannedBy = currentUser.Name;
            }
            await _db.SaveChangesAsync();
            
            // Логируем действие
            await _activityLogService.LogUserActivityAsync(
                currentUserId,
                "USER_BANNED_TEMPORARY",
                $"User {targetUser.Name} ({targetUser.Email}) was banned for {dto.Minutes} minutes by {currentUser.Name}",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                HttpContext.Request.Headers["User-Agent"].ToString()
            );
            
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
                
            return Ok(new { succeeded = true, message = $"Пользователь забанен на {period}." });
        }

        [HttpGet("{userId:int}")]
        public async Task<IActionResult> GetBanStatus(int userId)
        {
            var entry = await _db.UserBanEntries.FindAsync(userId);
            if (entry == null)
            {
                return Ok(new { isBanned = false, until = (string?)null });
            }

            // Проверяем, не истек ли временный бан
            if (entry.Until.HasValue && entry.Until.Value <= DateTime.UtcNow)
            {
                entry.IsBanned = false;
                entry.Until = null;
                await _db.SaveChangesAsync();
                return Ok(new { isBanned = false, until = (string?)null });
            }

            return Ok(new { 
                isBanned = entry.IsBanned, 
                until = entry.Until?.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                reason = entry.Reason,
                bannedBy = entry.BannedBy
            });
        }

        [HttpGet("my-ban-status")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMyBanStatus()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var entry = await _db.UserBanEntries.FindAsync(userId);
            
            if (entry == null)
            {
                return Ok(new { isBanned = false, until = (string?)null });
            }

            // Проверяем, не истек ли временный бан
            if (entry.Until.HasValue && entry.Until.Value <= DateTime.UtcNow)
            {
                entry.IsBanned = false;
                entry.Until = null;
                await _db.SaveChangesAsync();
                return Ok(new { isBanned = false, until = (string?)null });
            }

            return Ok(new { 
                isBanned = entry.IsBanned, 
                until = entry.Until?.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                reason = entry.Reason,
                bannedBy = entry.BannedBy
            });
        }

        private int GetRoleLevel(IList<string> roles)
        {
            return roles.Select(r => RoleHierarchy.GetValueOrDefault(r, 0)).DefaultIfEmpty(0).Max();
        }
    }
} 