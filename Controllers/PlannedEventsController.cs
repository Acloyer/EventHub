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
    public class PlannedEventsController : ControllerBase
    {
        private readonly EventHubDbContext _db;
        public PlannedEventsController(EventHubDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetPlanned()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var events = await _db.PlannedEvents
                .Where(p => p.UserId == userId)
                .Include(p => p.Event)
                .Select(p => p.Event)
                .ToListAsync();
            return Ok(events);
        }

        [HttpPost("{eventId}")]
        public async Task<IActionResult> AddToPlanned(int eventId)
        {
            int userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                ?? throw new Exception("User ID not found in token")
            );

            if (!await _db.Events.AnyAsync(e => e.Id == eventId))
                return NotFound("Event does not exist.");

            if (await _db.PlannedEvents.AnyAsync(p => p.UserId == userId && p.EventId == eventId))
                return BadRequest("Already planned");

            _db.PlannedEvents.Add(new PlannedEvent { UserId = userId, EventId = eventId });
            await _db.SaveChangesAsync();
            return Ok("Added to planned");
        }

        [HttpDelete("{eventId}")]
        public async Task<IActionResult> RemoveFromPlanned(int eventId)
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var plan = await _db.PlannedEvents
                .SingleOrDefaultAsync(p => p.UserId == userId && p.EventId == eventId);

            if (plan == null) return NotFound();
            _db.PlannedEvents.Remove(plan);
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}

