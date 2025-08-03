using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using EventHub.Data;
using EventHub.Models;
<<<<<<< HEAD
using EventHub.Services;
=======
<<<<<<< HEAD
using EventHub.Services;
=======
<<<<<<< HEAD
using EventHub.Services;
=======
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
using System.Security.Claims;

namespace EventHub.Controllers
{
    [ApiController]
    [Route("api/events/{eventId}/reactions")]
    public class ReactionsController : ControllerBase
    {
        private readonly EventHubDbContext _db;
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
        private readonly INotificationService _notificationService;

        public ReactionsController(EventHubDbContext db, INotificationService notificationService)
        {
            _db = db;
            _notificationService = notificationService;
        }
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
=======

        public ReactionsController(EventHubDbContext db)
            => _db = db;
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9

        [HttpDelete, Authorize]
        public async Task<IActionResult> RemoveMyReaction(int eventId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var reaction = await _db.PostReactions
                .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);
            if (reaction == null)
<<<<<<< HEAD
                return NotFound(new { message = "Reaction not found." });
=======
<<<<<<< HEAD
                return NotFound(new { message = "Reaction not found." });
=======
<<<<<<< HEAD
                return NotFound(new { message = "Reaction not found." });
=======
                return NotFound(new { message = "Реакция не найдена." });
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9

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
<<<<<<< HEAD
                return NotFound(new { message = "Reaction not found." });
=======
<<<<<<< HEAD
                return NotFound(new { message = "Reaction not found." });
=======
<<<<<<< HEAD
                return NotFound(new { message = "Reaction not found." });
=======
                return NotFound(new { message = "Реакция не найдена." });
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9

            _db.PostReactions.Remove(reaction);
            await _db.SaveChangesAsync();
            return NoContent();
        }

<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
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

<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
=======
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
        [HttpDelete("~/api/users/{userId}/reactions"), Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> RemoveAllReactionsForUser(int userId)
        {
            var reactions = await _db.PostReactions
                .Where(r => r.UserId == userId)
                .ToListAsync();
            if (!reactions.Any())
<<<<<<< HEAD
                return NotFound(new { message = "User reactions not found." });
=======
<<<<<<< HEAD
                return NotFound(new { message = "User reactions not found." });
=======
<<<<<<< HEAD
                return NotFound(new { message = "User reactions not found." });
=======
                return NotFound(new { message = "Реакции пользователя не найдены." });
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9

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
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
            
            // Check if user is muted
            var muteEntry = await _db.UserMuteEntries.FindAsync(userId);
            
            // Check if mute has expired
            if (muteEntry != null && muteEntry.IsMuted && muteEntry.Until.HasValue)
            {
                if (muteEntry.Until.Value <= DateTime.UtcNow)
                {
                    muteEntry.IsMuted = false;
                    muteEntry.Until = null;
                    _db.UserMuteEntries.Update(muteEntry);
                    await _db.SaveChangesAsync();
                }
            }
            
            // If user is currently muted, forbid adding reactions
            if (muteEntry?.IsMuted == true)
            {
                return Forbid("You are currently muted and cannot add reactions.");
            }
<<<<<<< HEAD
=======
=======
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9

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
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9

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

<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
=======
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
            return NoContent();
        }
    }

    public class ReactionDto
    {
        public string Emoji { get; set; }
    }
}
