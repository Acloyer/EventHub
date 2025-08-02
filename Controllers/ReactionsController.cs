using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using EventHub.Data;
using EventHub.Models;
using EventHub.Services;
using System.Security.Claims;

namespace EventHub.Controllers
{
    [ApiController]
    [Route("api/events/{eventId}/reactions")]
    public class ReactionsController : ControllerBase
    {
        private readonly EventHubDbContext _db;
        private readonly INotificationService _notificationService;

        public ReactionsController(EventHubDbContext db, INotificationService notificationService)
        {
            _db = db;
            _notificationService = notificationService;
        }

        [HttpDelete, Authorize]
        public async Task<IActionResult> RemoveMyReaction(int eventId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var reaction = await _db.PostReactions
                .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);
            if (reaction == null)
                return NotFound(new { message = "Reaction not found." });

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
                return NotFound(new { message = "Reaction not found." });

            _db.PostReactions.Remove(reaction);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // GET /api/events/{eventId}/reactions/counts
        [HttpGet("counts"), AllowAnonymous]
        public async Task<IActionResult> GetReactionCounts(int eventId)
        {
            // All possible emojis (in the same order as in your UI)
            var allEmojis = new[] { "👍", "👎", "❤️", "🎉", "😄", "😂", "😮", "😢", "😡", "👏", "🙌", "🔥", "😊" };

            // Counts from database
            var dbCounts = await _db.PostReactions
                .Where(r => r.EventId == eventId)
                .GroupBy(r => r.Emoji)
                .Select(g => new {
                    Emoji = g.Key,
                    Count = g.Count()
                })
                .ToDictionaryAsync(g => g.Emoji, g => g.Count);

            // Guaranteed to return all emojis
            var result = allEmojis.Select(emoji => new {
                Emoji = emoji,
                Count = dbCounts.ContainsKey(emoji) ? dbCounts[emoji] : 0
            });

            return Ok(result);
        }

        [HttpGet("user"), Authorize]
        public async Task<IActionResult> GetUserReaction(int eventId)
        {
            var userId = int.Parse(User.FindFirst("id")!.Value);
            
            var reaction = await _db.PostReactions
                .Where(r => r.EventId == eventId && r.UserId == userId)
                .Select(r => new { r.Emoji })
                .FirstOrDefaultAsync();

            // Always return 200 OK, even if no reaction exists
            return Ok(reaction);
        }

        [HttpDelete("~/api/users/{userId}/reactions"), Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> RemoveAllReactionsForUser(int userId)
        {
            var reactions = await _db.PostReactions
                .Where(r => r.UserId == userId)
                .ToListAsync();
            if (!reactions.Any())
                return NotFound(new { message = "User reactions not found." });

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

            // Create notification for event creator (if not the same user)
            var eventEntity = await _db.Events.FindAsync(eventId);
            if (eventEntity != null && eventEntity.CreatorId != userId)
            {
                await _notificationService.CreateNotificationAsync(
                    eventEntity.CreatorId,
                    "reaction",
                    eventId
                );
            }

            return NoContent();
        }
    }

    public class ReactionDto
    {
        public string Emoji { get; set; }
    }
}
