// Controllers/MutesController.cs
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using EventHub.Data;
using EventHub.Models.DTOs;
using EventHub.Models;
using System.Security.Claims; 
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;

namespace EventHub.Controllers
{
    [ApiController]
    [Route("api/users/{userId}/mute")]
    public class MutesController : ControllerBase
    {
        readonly EventHubDbContext _db;
        private readonly UserManager<User> _users;
        private readonly ITelegramBotClient _bot;
        public MutesController(EventHubDbContext db, UserManager<User> users, ITelegramBotClient bot)
        {
            _db = db;
            _users = users;
            _bot = bot;
        }

        [HttpPost, Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> SetMute(int userId, [FromBody] MuteDto dto)
        {
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

            var entry = await _db.UserMuteEntries.FindAsync(userId);
            if (entry == null)
            {
                entry = new UserMuteEntry { UserId = userId, IsMuted = dto.IsMuted };
                _db.UserMuteEntries.Add(entry);
            }
            else
            {
                entry.IsMuted = dto.IsMuted;
                if (!dto.IsMuted)
                {
                    entry.Until = null; // Reset mute time when unmuting
                }
            }
            await _db.SaveChangesAsync();
            
            // Отправляем уведомление в Telegram если пользователь замучен и у него есть Telegram
            if (dto.IsMuted && targetUser.TelegramId.HasValue && targetUser.IsTelegramVerified)
            {
                try
                {
                    var adminName = currentUser.Name ?? "Администратор";
                    var support = "Если вы считаете, что это ошибка — обратитесь в поддержку.";

                    var text = $"Вас замутил администратор {adminName}.\n" +
                               $"Тип: МУТ\n" +
                               $"Причина: не указана\n" +
                               $"До: навсегда (GMT+4)\n\n" +
                               $"{support}";

                    await _bot.SendTextMessageAsync(
                        chatId: targetUser.TelegramId.Value,
                        text: text
                    );
                }
                catch (Exception ex)
                {
                    // Можно добавить логирование
                    Console.WriteLine($"Ошибка отправки Telegram уведомления о муте: {ex.Message}");
                }
            }
            
            if(dto.IsMuted == false){
                return Ok(new { succeeded = true, message = "User unmuted." });
            }
            else{
                return Ok(new { succeeded = true, message = "User muted." });
            }
        }

        [HttpPost("duration"), Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> SetMuteDuration(int userId, [FromBody] MuteDurationDto dto)
        {
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

            var entry = await _db.UserMuteEntries.FindAsync(userId);
            if (entry == null)
            {
                entry = new UserMuteEntry { UserId = userId };
                _db.UserMuteEntries.Add(entry);
            }

            entry.IsMuted = true;
            var totalSeconds = dto.Seconds + (dto.Minutes * 60) + (dto.Hours * 3600);
            entry.Until = DateTime.UtcNow.AddSeconds(totalSeconds);

            await _db.SaveChangesAsync();
            
            // Отправляем уведомление в Telegram если у пользователя есть Telegram
            if (targetUser.TelegramId.HasValue && targetUser.IsTelegramVerified)
            {
                try
                {
                    var until = entry.Until?.ToUniversalTime().AddHours(4); // GMT+4
                    var untilStr = until?.ToString("dd.MM.yyyy HH:mm") ?? "навсегда";
                    var adminName = currentUser.Name ?? "Администратор";
                    var support = "Если вы считаете, что это ошибка — обратитесь в поддержку.";

                    var text = $"Вас замутил администратор {adminName}.\n" +
                               $"Тип: МУТ\n" +
                               $"Причина: не указана\n" +
                               $"До: {untilStr} (GMT+4)\n\n" +
                               $"{support}";

                    await _bot.SendTextMessageAsync(
                        chatId: targetUser.TelegramId.Value,
                        text: text
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка отправки Telegram уведомления о временном муте: {ex.Message}");
                }
            }

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

            return Ok(new
            {
                succeeded = true,
                message   = $"Пользователь «{name}» замучен на {period}."
            });
        }

        private int GetRoleLevel(IList<string> roles)
        {
            if (roles.Contains("Owner")) return 4;
            if (roles.Contains("SeniorAdmin")) return 3;
            if (roles.Contains("Admin")) return 2;
            if (roles.Contains("Organizer")) return 1;
            return 0; // User
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
