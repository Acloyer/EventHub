using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EventHub.Data;
using EventHub.Models;
using EventHub.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Controllers
{
    [ApiController]
    [Route("api/comments/{eventId}")]
    public class CommentsController : ControllerBase
    {
        private readonly EventHubDbContext _db;
        public CommentsController(EventHubDbContext db) => _db = db;

        // GET /api/events/{eventId}/comments
        [HttpGet, AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetAll(int eventId)
        {
            var comments = await _db.EventComments
                .Where(c => c.EventId == eventId)
                .OrderBy(c => c.PostDate)
                .Select(c => new CommentDto {
                    Id       = c.Id,
                    UserId   = c.UserId,
                    Comment  = c.Comment,
                    PostDate = c.PostDate,
                    IsEdited = c.IsEdited,
                    EditDate = c.EditDate,
                    IsPinned = c.IsPinned
                })
                .ToListAsync();

            return Ok(comments);
        }

        // GET /api/comments/{userId}
        [HttpGet("~/api/comments/{userId}"), Authorize]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetAllByUserId(int userId)
        {
            var comments = await _db.EventComments
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.PostDate)
                .Select(c => new CommentDto {
                    Id       = c.Id,
                    UserId   = c.UserId,
                    Comment  = c.Comment,
                    PostDate = c.PostDate,
                    IsEdited = c.IsEdited,
                    EditDate = c.EditDate,
                    IsPinned = c.IsPinned
                })
                .ToListAsync();

            return Ok(comments);
        }

        // POST /api/{eventId}/comments
        [HttpPost, Authorize]
        public async Task<IActionResult> Post(int eventId, [FromBody] CreateCommentDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var comment = new EventComment {
                EventId  = eventId,
                UserId   = userId,
                Comment  = dto.Comment,
                PostDate = DateTime.UtcNow
            };

            _db.EventComments.Add(comment);
            await _db.SaveChangesAsync();

            var result = new CommentDto {
                Id       = comment.Id,
                UserId   = comment.UserId,
                Comment  = comment.Comment,
                PostDate = comment.PostDate,
                IsEdited = false,
                IsPinned = false
            };

            return CreatedAtAction(nameof(GetAll), new { eventId }, result);
        }

        // PUT /api/comments/{commentId}
        [HttpPut("~/api/comments/{commentId}"), Authorize]
        public async Task<IActionResult> Update(int commentId, [FromBody] UpdateCommentDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var comment = await _db.EventComments.FindAsync(commentId);

            // if (comment == null || comment.EventId != eventId)
                // return NotFound();

            if (comment.UserId != userId)
                return Forbid();

            comment.Comment  = dto.Comment;
            comment.IsEdited = true;
            comment.EditDate = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok();
        }

        // DELETE /api/{eventId}/comments/{commentId}
        [HttpDelete("~/api/comments/{commentId}"), Authorize]
        public async Task<IActionResult> DeleteOwn(int commentId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var comment = await _db.EventComments.FindAsync(commentId);

            // if (comment == null || comment.EventId != eventId)
                // return NotFound();

            if (comment == null){
                return NotFound();
            }

            if (comment.UserId != userId)
                return Forbid();


            _db.EventComments.Remove(comment);
            await _db.SaveChangesAsync();
            return Ok();
        }

        // DELETE /api/events/{eventId}/comments/{commentId}/by-user/{userId}
        // — админы могут удалять чужие
        // ~/api/comments/admin/{commentId}
        [HttpDelete("~/api/comments/admin/{commentId}"), Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> DeleteAny(int commentId)
        {
            var comment = await _db.EventComments.FindAsync(commentId);
            // if (comment == null || comment.EventId != eventId || comment.UserId != userId)
            if (comment == null)
                return NotFound();

            _db.EventComments.Remove(comment);
            await _db.SaveChangesAsync();
            return Ok();
        }

        // (Опционально) PATCH /api/{eventId}/comments/{commentId}/pin
        [HttpPatch("~/api/comments/pin/{commentId}"), Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> Pin(int commentId, [FromQuery] bool pinned)
        {
            var comment = await _db.EventComments.FindAsync(commentId);
            // if (comment == null || comment.EventId != eventId)
            if (comment == null)
                return NotFound();

            comment.IsPinned = pinned;
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}
