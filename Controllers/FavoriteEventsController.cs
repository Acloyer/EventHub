using EventHub.Data;
using EventHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EventHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FavoriteEventsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public FavoriteEventsController(ApplicationDbContext db) => _db = db;

        // GET: api/FavoriteEvents
        [HttpGet]
        public async Task<IActionResult> GetFavorites()
        {
            var userId = int.Parse(
                User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
             ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var favorites = await _db.FavoriteEvents
                .Where(f => f.UserId == userId)
                .Include(f => f.Event)
                .Select(f => f.Event)
                .ToListAsync();

            return Ok(favorites);
        }

        // POST: api/FavoriteEvents/5
        [HttpPost("{eventId:int}")]
        public async Task<IActionResult> AddToFavorites(int eventId)
        {
            var userId = int.Parse(
                User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
             ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var @event = await _db.Events.FindAsync(eventId);
            if (@event == null)
                return NotFound("Event not found.");

            var existingFavorite = await _db.FavoriteEvents
                .FirstOrDefaultAsync(f => f.UserId == userId && f.EventId == eventId);

            if (existingFavorite != null)
            {
                _db.FavoriteEvents.Remove(existingFavorite);
                await _db.SaveChangesAsync();
                return Ok(new { isFavorite = false });
            }

            _db.FavoriteEvents.Add(new FavoriteEvent
            {
                UserId = userId,
                EventId = eventId
            });
            await _db.SaveChangesAsync();
            return Ok(new { isFavorite = true });
        }

        // DELETE: api/FavoriteEvents/5
        [HttpDelete("{eventId:int}")]
        public async Task<IActionResult> RemoveFromFavorites(int eventId)
        {
            var userId = int.Parse(
                User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
             ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var fav = await _db.FavoriteEvents
                .FirstOrDefaultAsync(f => f.UserId == userId && f.EventId == eventId);

            if (fav == null) return NotFound();
            _db.FavoriteEvents.Remove(fav);
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}
