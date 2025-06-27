using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EventHub.Data;
using EventHub.Models;
using EventHub.Models.DTOs;

namespace EventHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PlannedEventsController : ControllerBase
    {
        private readonly EventHubDbContext _db;

        public PlannedEventsController(EventHubDbContext db)
        {
            _db = db;
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

            var planned = await _db.PlannedEvents
                .Where(p => p.UserId == userId)
                .Include(p => p.Event)
                .Select(p => p.Event)
                .ToListAsync();

            return Ok(planned);
        }

        // POST: api/PlannedEvents/{eventId}
        [HttpPost("{eventId:int}")]
        public async Task<IActionResult> AddToPlanned(int eventId)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

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
        }
    }
}
