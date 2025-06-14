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
        private readonly ApplicationDbContext _db;
        public PlannedEventsController(ApplicationDbContext db) => _db = db;

        // GET: api/PlannedEvents
        [HttpGet]
        public async Task<IActionResult> GetPlanned()
        {
            var userId = int.Parse(
                User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
             ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var planned = await _db.PlannedEvents
                .Where(p => p.UserId == userId)
                .Include(p => p.Event)
                .Select(p => p.Event)
                .ToListAsync();

            return Ok(planned);
        }

        // POST: api/PlannedEvents/5
        [HttpPost("{eventId:int}")]
        public async Task<IActionResult> AddToPlanned(int eventId)
        {
            var userId = int.Parse(
                User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
             ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var @event = await _db.Events.FindAsync(eventId);
            if (@event == null)
                return NotFound("Event not found.");

            var existingPlanned = await _db.PlannedEvents
                .FirstOrDefaultAsync(p => p.UserId == userId && p.EventId == eventId);

            if (existingPlanned != null)
            {
                _db.PlannedEvents.Remove(existingPlanned);
                await _db.SaveChangesAsync();
                return Ok(new { isPlanned = false });
            }

            _db.PlannedEvents.Add(new PlannedEvent
            {
                UserId = userId,
                EventId = eventId
            });
            await _db.SaveChangesAsync();
            return Ok(new { isPlanned = true });
        }

        // DELETE: api/PlannedEvents/5
        [HttpDelete("{eventId:int}")]
        public async Task<IActionResult> RemoveFromPlanned(int eventId)
        {
            var userId = int.Parse(
                User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
             ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var plan = await _db.PlannedEvents
                .FirstOrDefaultAsync(p => p.UserId == userId && p.EventId == eventId);

            if (plan == null) return NotFound();
            _db.PlannedEvents.Remove(plan);
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}
