using EventHub.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EventHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly EventHubDbContext _db;
        public UserController(EventHubDbContext db) => _db = db;

        // Вспомогательный метод получения userId из токена
        private int GetUserId()
        {
            var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                   ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? throw new Exception("User ID not found in token");

            return int.Parse(sub);
        }

        // ==== Профиль ====

        // GET api/User/Profile
        [HttpGet("Profile")]
        public async Task<IActionResult> GetProfile()
        {
            int userId = GetUserId();

            var user = await _db.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            return Ok(new
            {
                user.Id,
                user.Email,
                Roles = user.UserRoles.Select(ur => ur.Role.Name),
                user.TelegramId,
                user.NotifyBeforeEvent
            });
        }

        // ==== Созданные, избранные, запланированные ====

        // GET api/User/CreatedEvents
        [HttpGet("CreatedEvents")]
        public async Task<IActionResult> GetCreatedEvents()
        {
            int userId = GetUserId();

            var events = await _db.Events
                .Where(e => e.OrganizerId == userId)
                .ToListAsync();

            return Ok(events);
        }

        // GET api/User/Favorites
        [HttpGet("Favorites")]
        public async Task<IActionResult> GetFavorites()
        {
            int userId = GetUserId();

            var favorites = await _db.FavoriteEvents
                .Where(f => f.UserId == userId)
                .Include(f => f.Event)
                .Select(f => f.Event)
                .ToListAsync();

            return Ok(favorites);
        }

        // GET api/User/Planned
        [HttpGet("Planned")]
        public async Task<IActionResult> GetPlanned()
        {
            int userId = GetUserId();

            var planned = await _db.PlannedEvents
                .Where(p => p.UserId == userId)
                .Include(p => p.Event)
                .Select(p => p.Event)
                .ToListAsync();

            return Ok(planned);
        }

        // ==== Telegram-привязка и уведомления ====

        // POST api/User/link-telegram/{telegramId}
        [HttpPost("link-telegram/{telegramId}")]
        public async Task<IActionResult> LinkTelegram(string telegramId)
        {
            int userId = GetUserId();
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.TelegramId = telegramId;
            await _db.SaveChangesAsync();
            return Ok("Telegram linked successfully");
        }

        // POST api/User/set-notify
        [HttpPost("set-notify")]
        public async Task<IActionResult> SetNotificationPreference([FromBody] bool notify)
        {
            int userId = GetUserId();
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.NotifyBeforeEvent = notify;
            await _db.SaveChangesAsync();
            return Ok(new { notify });
        }
    }
}
