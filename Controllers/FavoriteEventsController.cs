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
        private readonly EventHubDbContext _db;
        public FavoriteEventsController(EventHubDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetFavorites()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var events = await _db.FavoriteEvents
                .Where(f => f.UserId == userId)
                .Include(f => f.Event)
                .Select(f => f.Event)
                .ToListAsync();
            return Ok(events);
        }

        [HttpPost("{eventId}")]
        public async Task<IActionResult> AddToFavorites(int eventId)
        {
            int userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? throw new Exception("User ID not found")
            );

            if (!await _db.Events.AnyAsync(e => e.Id == eventId))
                return NotFound("Event does not exist.");

            if (await _db.FavoriteEvents.AnyAsync(f => f.UserId == userId && f.EventId == eventId))
                return BadRequest("Already added to favorites.");

            _db.FavoriteEvents.Add(new FavoriteEvent { UserId = userId, EventId = eventId });
            await _db.SaveChangesAsync();
            return Ok("Added to favorites");
        }

        [HttpDelete("{eventId}")]
        public async Task<IActionResult> RemoveFromFavorites(int eventId)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var fav = await _db.FavoriteEvents
                .SingleOrDefaultAsync(f => f.UserId == userId && f.EventId == eventId);

            if (fav == null) return NotFound();
            _db.FavoriteEvents.Remove(fav);
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}