using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using EventHub.Data;
using EventHub.Models;
using System.Security.Claims;

namespace EventHub.Controllers
{
    [ApiController]
    [Route("api/events/{eventId}/reactions")]
    public class ReactionsController : ControllerBase
    {
        private readonly EventHubDbContext _db;

        public ReactionsController(EventHubDbContext db)
            => _db = db;

        [HttpDelete, Authorize]
        public async Task<IActionResult> RemoveMyReaction(int eventId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var reaction = await _db.PostReactions
                .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);
            if (reaction == null)
                return NotFound(new { message = "Реакция не найдена." });

            _db.PostReactions.Remove(reaction);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{userId:int}"), Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> RemoveUserReaction(int eventId, int userId)
        {
            var reaction = await _db.PostReactions
                .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);
            if (reaction == null)
                return NotFound(new { message = "Реакция не найдена." });

            _db.PostReactions.Remove(reaction);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("~/api/users/{userId}/reactions"), Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> RemoveAllReactionsForUser(int userId)
        {
            var reactions = await _db.PostReactions
                .Where(r => r.UserId == userId)
                .ToListAsync();
            if (!reactions.Any())
                return NotFound(new { message = "Реакции пользователя не найдены." });

            _db.PostReactions.RemoveRange(reactions);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet, AllowAnonymous]
        public async Task<IActionResult> GetReactions(int eventId)
        {
            var reactions = await _db.PostReactions
                .Where(r => r.EventId == eventId)
                .Select(r => new { r.UserId, r.Emoji })
                .ToListAsync();

            return Ok(reactions);
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> AddOrUpdateReaction(int eventId, [FromBody] ReactionDto dto)
        {
            var userId = int.Parse(User.FindFirst("id")!.Value);

            var existing = await _db.PostReactions
                .SingleOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);

            if (existing != null)
            {
                existing.Emoji = dto.Emoji;
            }
            else
            {
                _db.PostReactions.Add(new PostReaction {
                    EventId = eventId,
                    UserId  = userId,
                    Emoji   = dto.Emoji
                });
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }
    }

    public class ReactionDto
    {
        public string Emoji { get; set; }
    }
}
