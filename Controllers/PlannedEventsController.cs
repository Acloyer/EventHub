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
    public class PlannedEventsController : ControllerBase
    {
        private readonly EventHubDbContext _db;
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
        private readonly ITelegramBotClient _bot;

        public PlannedEventsController(EventHubDbContext db, ITelegramBotClient bot)
        {
            _db = db;
            _bot = bot;
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
=======

        public PlannedEventsController(EventHubDbContext db)
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

        // GET: api/PlannedEvents
        [HttpGet]
        public async Task<IActionResult> GetPlanned()
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var plannedEvents = await _db.PlannedEvents
                .Where(p => p.UserId == userId)
                .Include(p => p.Event).ThenInclude(e => e.Creator)
                .Include(p => p.Event).ThenInclude(e => e.EventComments)
                .Select(p => new EventDto(p.Event)
                {
                    IsPlanned = true,
                    IsFavorite = p.Event.FavoriteEvents.Any(f => f.UserId == userId)
                })
                .ToListAsync();


            return Ok(plannedEvents);
        }

<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
        // POST: api/PlannedEvents/{eventId}
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
        [HttpPost("{eventId:int}")]
        public async Task<IActionResult> TogglePlanned(int eventId)
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
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return Unauthorized();

            var planned = await _db.PlannedEvents
                .FirstOrDefaultAsync(p => p.UserId == userId && p.EventId == eventId);

            bool isPlanned;

            if (planned != null)
            {
                _db.PlannedEvents.Remove(planned);
                isPlanned = false;
            }
            else
            {
                // Check if user is in organizer's blacklist
                var isBlacklisted = await _db.OrganizerBlacklists
                    .AnyAsync(ob => ob.OrganizerId == ev.CreatorId && ob.BannedUserId == userId);

                if (isBlacklisted)
                {
                    return Forbid("You are banned from attending events by this organizer.");
                }

                _db.PlannedEvents.Add(new PlannedEvent { UserId = userId, EventId = eventId, CreatedAt = DateTime.UtcNow });
                isPlanned = true;
            }

            await _db.SaveChangesAsync();

            // 🔔 Telegram notification if enabled
            if (user.TelegramId != null && user.IsTelegramVerified && user.NotifyBeforeEvent)
            {
                var message = isPlanned
                    ? $"🗓 <b>You added</b> event <i>«{ev.Title}»</i> to planned!"
                    : $"❌ <b>You removed</b> event <i>«{ev.Title}»</i> from planned.";

                try
                {
                    await _bot.SendTextMessageAsync(
                        chatId: user.TelegramId.Value,
                        text: message,
                        parseMode: ParseMode.Html
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Telegram error: {ex.Message}");
                    // Don't block execution
                }
            }

            return Ok(new { isPlanned });
<<<<<<< HEAD
        }



=======
<<<<<<< HEAD
        }



=======
        }



=======
            var exists = await _db.PlannedEvents
                .AnyAsync(p => p.UserId == userId && p.EventId == eventId);

            if (!exists)
            {
                _db.PlannedEvents.Add(new PlannedEvent { UserId = userId, EventId = eventId });
                await _db.SaveChangesAsync();
            }

            return Ok(new { isPlanned = true });
        }

>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
        // DELETE: api/PlannedEvents/{eventId}
        [HttpDelete("{eventId:int}")]
        public async Task<IActionResult> RemoveFromPlanned(int eventId)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var plan = await _db.PlannedEvents
                .FirstOrDefaultAsync(p => p.UserId == userId && p.EventId == eventId);

            if (plan == null)
                return NotFound("Planned entry not found.");

            _db.PlannedEvents.Remove(plan);
            await _db.SaveChangesAsync();

            return Ok(new { isPlanned = false });
        }

        // ===== Admin endpoints =====

        // GET: api/PlannedEvents/admin/all
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> GetAllPlannedEvents()
        {
            var all = await _db.PlannedEvents
                .Include(p => p.Event)
                    .ThenInclude(e => e.Creator)
                .ToListAsync();

            var result = all.Select(p => new
            {
                p.UserId,
                p.EventId,
                // Event = new EventDto(p.Event)
            }).ToList();

            return Ok(result);
        }

        // GET: api/PlannedEvents/user/{userId}
        [HttpGet("user/{userId:int}")]
        [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> GetPlannedForUser(int userId)
        {
            var list = await _db.PlannedEvents
                .Where(p => p.UserId == userId)
                .Include(p => p.Event)
                    .ThenInclude(e => e.Creator)
                .Select(p => new EventDto(p.Event))
                .ToListAsync();

            return Ok(list);
        }

        // POST: api/PlannedEvents/user/{userId}/{eventId}
        [HttpPost("user/{userId:int}/{eventId:int}")]
        [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> AddPlannedForUser(int userId, int eventId)
        {
            var ev = await _db.Events.FindAsync(eventId);
            if (ev == null)
                return NotFound("Event not found.");

            var exists = await _db.PlannedEvents
                .AnyAsync(p => p.UserId == userId && p.EventId == eventId);
            if (!exists)
            {
                _db.PlannedEvents.Add(new PlannedEvent { UserId = userId, EventId = eventId });
                await _db.SaveChangesAsync();
            }

            return Ok(new { isPlanned = true });
        }

        // DELETE: api/PlannedEvents/user/{userId}/{eventId}
        [HttpDelete("user/{userId:int}/{eventId:int}")]
        [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> RemovePlannedForUser(int userId, int eventId)
        {
            var plan = await _db.PlannedEvents
                .FirstOrDefaultAsync(p => p.UserId == userId && p.EventId == eventId);
            if (plan == null)
                return NotFound("Planned entry not found.");

            _db.PlannedEvents.Remove(plan);
            await _db.SaveChangesAsync();

            return Ok(new { isPlanned = false });
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
        }

        [HttpGet("event/{eventId:int}/attendees")]
        [Authorize(Roles = "Admin,SeniorAdmin,Owner,Organizer")]
        public async Task<IActionResult> GetEventAttendees(int eventId)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            // Проверяем, что событие существует
            var ev = await _db.Events.FindAsync(eventId);
            if (ev == null)
                return NotFound("Event not found.");

            // Проверяем права доступа: только создатель события или админы могут видеть участников
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return Unauthorized();

            var userRoles = await _db.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .ToListAsync();

            var isAdmin = userRoles.Any(r => r == "Admin" || r == "SeniorAdmin" || r == "Owner");
            var isOrganizer = userRoles.Any(r => r == "Organizer");
            var isEventCreator = ev.CreatorId == userId;

            if (!isAdmin && !isEventCreator)
                return Forbid("You don't have permission to view attendees for this event.");

            var attendees = await _db.PlannedEvents
                .Where(p => p.EventId == eventId)
                .Include(p => p.User)
                .Select(p => new
                {
                    UserId = p.User.Id,
                    UserName = p.User.Name,
                    UserEmail = p.User.Email,
                    AddedAt = p.CreatedAt ?? DateTime.UtcNow // Если CreatedAt не установлен, используем текущее время
                })
                .ToListAsync();

            return Ok(new
            {
                EventId = eventId,
                EventTitle = ev.Title,
                AttendeesCount = attendees.Count,
                Attendees = attendees
            });
        }

        [HttpDelete("event/{eventId:int}/attendee/{attendeeId:int}")]
        [Authorize(Roles = "Admin,SeniorAdmin,Owner,Organizer")]
        public async Task<IActionResult> RemoveAttendeeFromEvent(int eventId, int attendeeId)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            // Проверяем, что событие существует
            var ev = await _db.Events.FindAsync(eventId);
            if (ev == null)
                return NotFound("Event not found.");

            // Проверяем права доступа: только создатель события или админы могут удалять участников
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return Unauthorized();

            var userRoles = await _db.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                .ToListAsync();

            var isAdmin = userRoles.Any(r => r == "Admin" || r == "SeniorAdmin" || r == "Owner");
            var isEventCreator = ev.CreatorId == userId;

            if (!isAdmin && !isEventCreator)
                return Forbid("You don't have permission to remove attendees from this event.");

            // Проверяем, что участник существует
            var plannedEvent = await _db.PlannedEvents
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.EventId == eventId && p.UserId == attendeeId);

            if (plannedEvent == null)
                return NotFound("Attendee not found for this event.");

            // Удаляем участника из события
            _db.PlannedEvents.Remove(plannedEvent);
            await _db.SaveChangesAsync();

            return Ok(new { message = $"User {plannedEvent.User.Name} removed from event successfully." });
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
=======
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
        }
    }
}
