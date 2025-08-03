using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EventHub.Data;
using EventHub.Models;
using EventHub.Models.DTOs;
<<<<<<< HEAD
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
=======
<<<<<<< HEAD
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
=======
<<<<<<< HEAD
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
=======
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9

namespace EventHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FavoriteEventsController : ControllerBase
    {
        private readonly EventHubDbContext _db;
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
        private readonly ITelegramBotClient    _bot;

        public FavoriteEventsController(EventHubDbContext db, ITelegramBotClient bot)
        {
            _db = db;
            _bot = bot;
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
=======

        public FavoriteEventsController(EventHubDbContext db)
        {
            _db = db;
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
        }

        private bool TryGetUserId(out int userId)
        {
            var raw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(raw, out userId);
        }

        // GET: api/FavoriteEvents
        [HttpGet]
        public async Task<IActionResult> GetFavorites()
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var favoritesEvents = await _db.FavoriteEvents
                .Where(p => p.UserId == userId)
                .Include(p => p.Event).ThenInclude(e => e.Creator)
                .Include(p => p.Event).ThenInclude(e => e.EventComments)
                .Select(p => new EventDto(p.Event)
                {
                    // IsPlanned = true,
                    IsPlanned = p.Event.PlannedEvents.Any(f => f.UserId == userId),
                    IsFavorite = p.Event.FavoriteEvents.Any(f => f.UserId == userId)
                })
                .ToListAsync();

            return Ok(favoritesEvents);
        }

        // POST: api/FavoriteEvents/{eventId}
        [HttpPost("{eventId:int}")]
        public async Task<IActionResult> ToggleFavorite(int eventId)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var ev = await _db.Events.FindAsync(eventId);
            if (ev == null)
                return NotFound("Event not found.");

<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
            var favorite = await _db.FavoriteEvents
                .FirstOrDefaultAsync(f => f.UserId == userId && f.EventId == eventId);

            if (favorite != null)
            {
                _db.FavoriteEvents.Remove(favorite);
                await _db.SaveChangesAsync();

                var user = await _db.Users.FindAsync(userId);
                if (user != null && user.TelegramId.HasValue && user.IsTelegramVerified && user.NotifyBeforeEvent)
                {
<<<<<<< HEAD
                    var message = $"❌ <b>You removed event</b> «<i>{ev.Title}</i>» from favorites.";
=======
<<<<<<< HEAD
                    var message = $"❌ <b>You removed event</b> «<i>{ev.Title}</i>» from favorites.";
=======
                    var message = $"❌ <b>Вы добавили событие</b> «<i>{ev.Title}</i>» удалено из избранного.";
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
                    await _bot.SendTextMessageAsync(
                        chatId: user.TelegramId.Value,
                        text: message,
                        parseMode: ParseMode.Html
                    );
                }
                // _logger.LogInformation("UserId={UserId}, TelegramId={TelegramId}, Verified={Verified}, Notify={Notify}",
                // user.Id, user.TelegramId, user.IsTelegramVerified, user.NotifyBeforeEvent);
                Console.WriteLine($"[TelegramDebug] userId: {user?.Id}, telegramId: {user?.TelegramId}, verified: {user?.IsTelegramVerified}, notify: {user?.NotifyBeforeEvent}");

                return Ok(new { isFavorite = false });
            }

            else
            {
                _db.FavoriteEvents.Add(new FavoriteEvent { UserId = userId, EventId = eventId });
                await _db.SaveChangesAsync();

                var user = await _db.Users.FindAsync(userId);
                if (user != null && user.TelegramId.HasValue && user.IsTelegramVerified && user.NotifyBeforeEvent)
                {
<<<<<<< HEAD
                    var message = $"⭐ <b>You added event</b> «<i>{ev.Title}</i>» to favorites!";
=======
<<<<<<< HEAD
                    var message = $"⭐ <b>You added event</b> «<i>{ev.Title}</i>» to favorites!";
=======
                    var message = $"⭐ <b>Вы добавили</b> событие «<i>{ev.Title}</i>» добавлено в избранное!";
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
                    await _bot.SendTextMessageAsync(
                        chatId: user.TelegramId.Value,
                        text: message,
                        parseMode: ParseMode.Html
                    );
                }
                
                // _logger.LogInformation("UserId={UserId}, TelegramId={TelegramId}, Verified={Verified}, Notify={Notify}",
                // user.Id, user.TelegramId, user.IsTelegramVerified, user.NotifyBeforeEvent);
                Console.WriteLine($"[TelegramDebug] userId: {user?.Id}, telegramId: {user?.TelegramId}, verified: {user?.IsTelegramVerified}, notify: {user?.NotifyBeforeEvent}");

                return Ok(new { isFavorite = true });
            }
        }

        // POST: api/FavoriteEvents/{eventId}/{isFavorite}
        [HttpPost("{eventId:int}/{isFavorite:bool}")]
        public async Task<IActionResult> AddToFavorites(int eventId, bool isFavorite)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var ev = await _db.Events.FindAsync(eventId);
            if (ev == null)
                return NotFound("Event not found.");

            var exists = await _db.FavoriteEvents
                .AnyAsync(f => f.UserId == userId && f.EventId == eventId);

            if (!exists)
            {
                _db.FavoriteEvents.Add(new FavoriteEvent { UserId = userId, EventId = eventId });
                await _db.SaveChangesAsync();
            }

            // TEMP: Убери if и просто протестиру

<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
=======
            var exists = await _db.FavoriteEvents
                .AnyAsync(f => f.UserId == userId && f.EventId == eventId);

            if (!exists)
            {
                _db.FavoriteEvents.Add(new FavoriteEvent { UserId = userId, EventId = eventId });
                await _db.SaveChangesAsync();
            }

>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
            return Ok(new { isFavorite = true });
        }

        // DELETE: api/FavoriteEvents/{eventId}
        [HttpDelete("{eventId:int}")]
        public async Task<IActionResult> RemoveFromFavorites(int eventId)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var fav = await _db.FavoriteEvents
                .FirstOrDefaultAsync(f => f.UserId == userId && f.EventId == eventId);

            if (fav == null)
                return NotFound("Favorite entry not found.");

            _db.FavoriteEvents.Remove(fav);
            await _db.SaveChangesAsync();

            return Ok(new { isFavorite = false });
        }

        // ===== Admin endpoints =====

<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
        // GET: api/FavoriteEvents/admin/all
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> GetAllFavorites()
        {
            var all = await _db.FavoriteEvents
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
                .Include(f => f.User)
                .Include(f => f.Event).ThenInclude(e => e.Creator)
                .ToListAsync();

            var result = all
                .Select(f => new
                {
                    UserId        = f.UserId,
                    UserName      = f.User.UserName,      // или f.User.FullName
                    UserEmail     = f.User.Email,
                    EventId       = f.EventId,
                    Event         = new EventDto(f.Event)
                })
                .ToList();
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
=======
                .Include(f => f.Event)
                    .ThenInclude(e => e.Creator)
                .ToListAsync();

            var result = all.Select(f => new
            {
                f.UserId,
                f.EventId,
                Event = new EventDto(f.Event)
            }).ToList();
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9

            return Ok(result);
        }

        // GET: api/FavoriteEvents/user/{userId}
        [HttpGet("user/{userId:int}")]
        [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> GetFavoritesForUser(int userId)
        {
            var list = await _db.FavoriteEvents
                .Where(f => f.UserId == userId)
                .Include(f => f.Event)
                    .ThenInclude(e => e.Creator)
                .Select(f => new EventDto(f.Event))
                .ToListAsync();

            return Ok(list);
        }

        // POST: api/FavoriteEvents/user/{userId}/{eventId}
        [HttpPost("user/{userId:int}/{eventId:int}")]
        [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> AddFavoriteForUser(int userId, int eventId)
        {
            var ev = await _db.Events.FindAsync(eventId);
            if (ev == null)
                return NotFound("Event not found.");

            var exists = await _db.FavoriteEvents
                .AnyAsync(f => f.UserId == userId && f.EventId == eventId);
            if (!exists)
            {
                _db.FavoriteEvents.Add(new FavoriteEvent { UserId = userId, EventId = eventId });
                await _db.SaveChangesAsync();
            }

            return Ok(new { isFavorite = true });
        }

        // DELETE: api/FavoriteEvents/user/{userId}/{eventId}
        [HttpDelete("user/{userId:int}/{eventId:int}")]
        [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> RemoveFavoriteForUser(int userId, int eventId)
        {
            var fav = await _db.FavoriteEvents
                .FirstOrDefaultAsync(f => f.UserId == userId && f.EventId == eventId);
            if (fav == null)
                return NotFound("Favorite entry not found.");

            _db.FavoriteEvents.Remove(fav);
            await _db.SaveChangesAsync();

            return Ok(new { isFavorite = false });
        }
    }
}