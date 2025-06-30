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
    public class FavoriteEventsController : ControllerBase
    {
        private readonly EventHubDbContext _db;

        public FavoriteEventsController(EventHubDbContext db)
        {
            _db = db;
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

            var favorites = await _db.FavoriteEvents
                .Where(f => f.UserId == userId)
                .Include(f => f.Event)
                .Select(f => f.Event)
                .ToListAsync();

            return Ok(favorites);
        }

        // POST: api/FavoriteEvents/{eventId}
        [HttpPost("{eventId:int}")]
        public async Task<IActionResult> AddToFavorites(int eventId)
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

        // GET: api/FavoriteEvents/admin/all
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> GetAllFavorites()
        {
            var all = await _db.FavoriteEvents
                .Include(f => f.Event)
                    .ThenInclude(e => e.Creator)
                .ToListAsync();

            var result = all.Select(f => new
            {
                f.UserId,
                f.EventId,
                Event = new EventDto(f.Event)
            }).ToList();

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