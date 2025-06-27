using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EventHub.Data;
using EventHub.Models;
using EventHub.Models.DTOs;
using EventHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Telegram.Bot;
using Telegram.Bot.Types;
using User = EventHub.Models.User;
using WTelegram;

namespace EventHub.Controllers
{
    [Route("api/User")]
    [Authorize]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly EventHubDbContext _db;
        private readonly IUserService _userService;
        private readonly UserManager<User> _userManager;
        private readonly ITelegramBotClient _bot;
        private readonly ILogger<UserController> _logger;
        private readonly IConfiguration       _config;
        public UserController(
            EventHubDbContext db,
            IUserService userService,
            UserManager<User> userManager,
            ITelegramBotClient bot,
            ILogger<UserController> logger,
            IConfiguration config)
        {
            _db = db;
            _userService = userService;
            _userManager = userManager;
            _bot = bot;
            _logger = logger;
            _config = config;
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }

        /// <summary>
        /// Вытаскивает userId из JWT-клейма NameIdentifier
        /// </summary>
        private bool TryGetUserId(out int userId)
        {
            var raw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(raw, out userId);
        }

        /// <summary>
        /// GET: api/User/profile
        /// Возвращает профиль по userId из токена
        /// </summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                IsBanned = user.IsBanned,
                Roles = roles.ToArray(),
                TelegramId = user.TelegramId,
                IsTelegramVerified = user.IsTelegramVerified,
                NotifyBeforeEvent = user.NotifyBeforeEvent
            });
        }

        /// <summary>
        /// GET: api/User/created-events
        /// Возвращает события, созданные текущим пользователем
        /// </summary>
        [HttpGet("created-events")]
        public async Task<IActionResult> GetCreatedEvents()
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var events = await _db.Events
                .Include(e => e.Creator)
                .Where(e => e.CreatorId == userId)
                .ToListAsync();

            return Ok(events.Select(e => new EventDto(e)));
        }

        [Authorize]
        [HttpPost("set-notify")]
        public async Task<IActionResult> SetNotify([FromBody] bool notify)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return NotFound();

            user.NotifyBeforeEvent = notify;
            await _userManager.UpdateAsync(user);
            return Ok(new { notify });
        }

        // [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        [HttpGet("all")]
        [Authorize]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _db.Users.ToListAsync();

            var result = new List<UserDto>();
            foreach (var user in users)
            {
                var dto = new UserDto(user){
                    IsBanned = user.IsBanned
                };
                dto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
                result.Add(dto);
            }

            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();
            return Ok(new UserDto(user));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var currentUser = GetCurrentUserId();
            if (currentUser == id) return BadRequest("Cannot delete self");

            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            await _userManager.DeleteAsync(user);
            return NoContent();
        }

        [HttpPost("{id}/toggle-ban")]
        [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> ToggleBan(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();
            user.IsBanned = !user.IsBanned;
            await _userManager.UpdateAsync(user);
            return Ok(new { user.IsBanned });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto dto)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null) return NotFound();

            user.Name = dto.Name;
            user.IsBanned = dto.IsBanned;
            user.TelegramId = dto.TelegramId;
            user.IsTelegramVerified = dto.IsTelegramVerified;
            user.NotifyBeforeEvent = dto.NotifyBeforeEvent;

            await _userManager.UpdateAsync(user);
            return NoContent();
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileUpdateDto dto)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return NotFound();

            // Применяем только безопасные для себя поля:
            if (!string.IsNullOrWhiteSpace(dto.Name))
                user.Name = dto.Name;

            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {
                if (await _userManager.FindByEmailAsync(dto.Email) != null)
                    return BadRequest("Email уже занят");
                user.Email    = dto.Email;
                user.UserName = dto.Email;
            }

            user.TelegramId        = dto.TelegramId;
            user.NotifyBeforeEvent = dto.NotifyBeforeEvent;
            // user.IsBanned не трогаем!

            var res = await _userManager.UpdateAsync(user);
            if (!res.Succeeded) return BadRequest(res.Errors);

            return Ok(new UserDto(user));
        }

        [Authorize]
        [HttpGet("link-telegram")]
        public IActionResult GetTelegramLink()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var botUsername = "eventthub_bot"; 
            var link = $"https://t.me/{botUsername}?start={userId.Value}";
            return Ok(new { link });
        }

        [Authorize]
        [HttpPost("start-telegram-verification")]
        public async Task<IActionResult> StartTelegramVerification()
        {
            var userId = GetCurrentUserId();
            if (userId == null) 
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) 
                return NotFound();

            if (user.TelegramId == null)
                return BadRequest(new { message = "Сначала привяжите Telegram (link-telegram)." });

            if (user.IsTelegramVerified)
                return BadRequest(new { message = "Вы уже верифицированы." });

            // Генерируем шестизначный код
            var code    = new Random().Next(100000, 999999);
            var expires = DateTime.UtcNow.AddMinutes(10);

            // Сохраняем в базу
            var tv = new TelegramVerification 
            {
                UserId    = user.Id,
                ChatId    = user.TelegramId.Value,
                Code      = code,
                ExpiresAt = expires
            };
            _db.TelegramVerifications.Add(tv);
            await _db.SaveChangesAsync();

            // Отправляем код в Telegram
            await _bot.SendTextMessageAsync(
                chatId: user.TelegramId.Value,
                text: $"Ваш код для верификации: {code} (действует до {expires:u})"
            );

            return Ok(new 
            {
                message   = "Код отправлен в Telegram. Введите его в чате с ботом."
            });
        }

    }
}