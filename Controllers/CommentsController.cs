using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EventHub.Data;
using EventHub.Models;
using EventHub.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using EventHub.Services;
using UserModel = EventHub.Models.User;

namespace EventHub.Controllers
{
    [ApiController]
    // [Route("api/comments/{eventId}")]

    [Route("api/comments")]
    public class CommentsController : ControllerBase
    {
        private readonly EventHubDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly IActivityLogService _activityLogService;
        private readonly INotificationService _notificationService;

        public CommentsController(
            EventHubDbContext db,
            UserManager<User> userManager,
            IActivityLogService activityLogService,
            INotificationService notificationService)
        {
            _db = db;
            _userManager = userManager;
            _activityLogService = activityLogService;
            _notificationService = notificationService;
        }

        private string GetUserAgent()
        {
            return HttpContext.Request.Headers["User-Agent"].ToString() ?? "Unknown";
        }

        // Controllers/CommentsController.cs

        // GET api/comments
        [HttpGet("all"), Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<ActionResult<PaginatedResponse<CommentDto>>> GetAllComments(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            // 1) Base query with Include and sorting
            var query = _db.EventComments
                .Include(c => c.User)
                .Include(c => c.Event)
                .OrderByDescending(c => c.IsPinned)
                .ThenByDescending(c => c.PinnedAt)
                .ThenBy(c => c.PostDate);

            // 2) Count total and calculate number of pages
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // 3) Select only the needed page
            var pageComments = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 4) Get roles list for each unique UserId
            var userIds = pageComments.Select(c => c.UserId).Distinct();
            var rolesDict = new Dictionary<int, IList<string>>();
            foreach (var uid in userIds)
            {
                var user = pageComments.First(c => c.UserId == uid).User!;
                var roles = await _userManager.GetRolesAsync(user);
                rolesDict[uid] = roles;
            }

            // 5) Project comments to DTO
            var dtos = pageComments.Select(c => new CommentDto
            {
                Id         = c.Id,
                EventId    = c.EventId,
                EventTitle = c.Event?.Title  ?? "",
                UserId     = c.UserId,
                UserName   = c.User?.Name     ?? "",
                UserRoles  = rolesDict[c.UserId].ToList(),
                Comment    = c.Comment,
                PostDate   = c.PostDate,
                IsEdited   = c.IsEdited,
                EditDate   = c.EditDate,
                IsPinned   = c.IsPinned,
                PinnedAt   = c.PinnedAt
            }).ToList();

            // 6) Return PaginatedResponse
            return Ok(new PaginatedResponse<CommentDto>
            {
                Items      = dtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize   = pageSize,
                TotalPages = totalPages
            });
        }
 
        // GET api/comments/{eventId}
        [HttpGet("{eventId:int}"), AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetByEvent(int eventId)
        {
            // 1) First extract comments with navigation to User and Event
            var comments = await _db.EventComments
                .Where(c => c.EventId == eventId)
                .Include(c => c.User)
                .Include(c => c.Event)
                .ToListAsync();

            // 2) Get roles list for each unique userId through UserManager
            var userIds = comments.Select(c => c.UserId).Distinct();
            var rolesDict = new Dictionary<int, IList<string>>();
            foreach (var uid in userIds)
            {
                var user = comments.First(c => c.UserId == uid).User!;
                var roles = await _userManager.GetRolesAsync(user);
                rolesDict[uid] = roles;
            }

            // 3) Project to DTO, substituting roles list from dictionary for each record
            var dtos = comments
                .OrderByDescending(c => c.IsPinned)
                .ThenByDescending(c => c.PinnedAt)
                .ThenBy(c => c.PostDate)
                .Select(c => new CommentDto
                {
                    Id         = c.Id,
                    EventId    = c.EventId,
                    EventTitle = c.Event?.Title  ?? "",
                    UserId     = c.UserId,
                    UserName   = c.User?.Name     ?? "",
                    UserRoles  = rolesDict[c.UserId].ToList(),
                    Comment    = c.Comment,
                    PostDate   = c.PostDate,
                    IsEdited   = c.IsEdited,
                    EditDate   = c.EditDate,
                    IsPinned   = c.IsPinned,
                    PinnedAt   = c.PinnedAt
                })
                .ToList();

            return Ok(dtos);
        }
        // 2.3 Create comment
        // POST /api/comments/{eventId}
        // Controllers/CommentsController.cs

        // POST api/comments/{eventId}
        [HttpPost("{eventId:int}"), Authorize]
        public async Task<IActionResult> Create(int eventId, [FromBody] CreateCommentDto dto)
        {
            // Check if user is muted
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
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
            
            // If user is currently muted, forbid comment creation
            if (muteEntry?.IsMuted == true)
            {
                return Forbid("You are currently muted and cannot create comments.");
            }

            // 1) Save new comment
            var comment = new EventComment
            {
                EventId  = eventId,
                UserId   = userId,
                Comment  = dto.Comment,
                PostDate = DateTime.UtcNow
            };
            _db.EventComments.Add(comment);
            await _db.SaveChangesAsync();

            // Log comment creation
            await _activityLogService.LogCommentActivityAsync(
                userId,
                "COMMENT_CREATED",
                comment.Id,
                $"Created comment on event '{comment.Event?.Title ?? "Unknown"}'",
                GetUserAgent()
            );

            // Create notification for event creator (if not the same user)
            await _db.Entry(comment).Reference(c => c.Event).LoadAsync();
            if (comment.Event != null && comment.Event.CreatorId != userId)
            {
                await _notificationService.CreateNotificationAsync(
                    comment.Event.CreatorId,
                    "comment",
                    comment.Id
                );
            }

            // 2) Load User and Event to fill DTO
            await _db.Entry(comment).Reference(c => c.User).LoadAsync();

            // 3) Get roles list through UserManager
            var roles = await _userManager.GetRolesAsync(comment.User!);

            // 4) Manual mapping to CommentDto
            var resultDto = new CommentDto
            {
                Id         = comment.Id,
                EventId    = comment.EventId,
                EventTitle = comment.Event?.Title  ?? "",
                UserId     = comment.UserId,
                UserName   = comment.User?.Name    ?? "",
                UserRoles  = roles.ToList(),
                Comment    = comment.Comment,
                PostDate   = comment.PostDate,
                IsEdited   = comment.IsEdited,
                EditDate   = comment.EditDate,
                IsPinned   = comment.IsPinned,
                PinnedAt   = comment.PinnedAt
            };

            // 5) Return 201 Created with DTO body
            return CreatedAtAction(
                nameof(GetByEvent),
                new { eventId = comment.EventId },
                resultDto
            );
        }

        // 2.4 Update (own)
        // PUT /api/comments/{commentId}
        [HttpPut("{commentId}"), Authorize]
        public async Task<IActionResult> Update(int commentId, [FromBody] UpdateCommentDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
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
            
            // If user is currently muted, forbid comment updates
            if (muteEntry?.IsMuted == true)
            {
                return Forbid("You are currently muted and cannot update comments.");
            }
            
            var comment = await _db.EventComments.Include(x => x.Event).FirstOrDefaultAsync(x => x.Id == commentId);
            if (comment == null) return NotFound();
            
            if (comment.IsPinned) return Forbid(); // Cannot edit pinned comments
            
            // Get current user and their roles
            var currentUser = await _userManager.GetUserAsync(User);
            var currentRoles = await _userManager.GetRolesAsync(currentUser);
            
            // Get comment author and their roles
            var author = await _userManager.FindByIdAsync(comment.UserId.ToString());
            var authorRoles = await _userManager.GetRolesAsync(author);
            
            // Get event creator and their roles
            var eventCreator = await _userManager.FindByIdAsync(comment.Event.CreatorId.ToString());
            var eventCreatorRoles = await _userManager.GetRolesAsync(eventCreator);
            
            // Role hierarchy
            var hierarchy = new Dictionary<string, int> {
                ["User"] = 0,
                ["Organizer"] = 1,
                ["Admin"] = 2,
                ["SeniorAdmin"] = 3,
                ["Owner"] = 4
            };
            
            int currentMax = currentRoles.Where(r => hierarchy.ContainsKey(r)).DefaultIfEmpty("User").Max(r => hierarchy[r]);
            int authorMax = authorRoles.Where(r => hierarchy.ContainsKey(r)).DefaultIfEmpty("User").Max(r => hierarchy[r]);
            int eventCreatorMax = eventCreatorRoles.Where(r => hierarchy.ContainsKey(r)).DefaultIfEmpty("User").Max(r => hierarchy[r]);
            
            // Owner can edit any comment
            if (currentRoles.Contains("Owner"))
            {
                // Allow editing
            }
            // SeniorAdmin can edit comments in events below their rank (except Owner events)
            else if (currentRoles.Contains("SeniorAdmin"))
            {
                if (eventCreatorMax >= 4) // Owner event
                {
                    return new ContentResult
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        Content = "Cannot edit comments in Owner events",
                        ContentType = "text/plain"
                    };
                }
            }
            // Admin can edit comments only in their own events and events below their rank
            else if (currentRoles.Contains("Admin"))
            {
                // Check if it's their own event
                if (currentUser.Id != comment.Event.CreatorId)
                {
                    return new ContentResult
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        Content = "Admins can only edit comments in their own events",
                        ContentType = "text/plain"
                    };
                }
            }
            // Regular users can only edit their own comments
            else
            {
                if (comment.UserId != userId)
                {
                    return Forbid();
                }
            }
            
            // Store old comment for logging
            var oldComment = comment.Comment;
            
            comment.Comment = dto.Comment;
            comment.IsEdited = true;
            comment.EditDate = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            
            // Log the comment update with detailed information
            var userAgent = GetUserAgent();
            var details = $"Updated comment {commentId} in event '{comment.Event.Title}' (ID: {comment.Event.Id}). Old comment: '{oldComment}'. New comment: '{dto.Comment}'";
            await _activityLogService.LogCommentActivityAsync(userId, "Update", commentId, details, userAgent);
            
            return Ok();
        }

        // 2.5 Delete own
        // DELETE /api/comments/{commentId}
        [HttpDelete("{commentId}"), Authorize]
        public async Task<IActionResult> DeleteOwn(int commentId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
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
            
            // If user is currently muted, forbid comment deletion
            if (muteEntry?.IsMuted == true)
            {
                return Forbid("You are currently muted and cannot delete comments.");
            }
            
            var comment = await _db.EventComments.Include(x => x.Event).FirstOrDefaultAsync(x => x.Id == commentId);
            if (comment == null) return NotFound();
            
            // Get current user and their roles
            var currentUser = await _userManager.GetUserAsync(User);
            var currentRoles = await _userManager.GetRolesAsync(currentUser);
            
            // Get event creator and their roles
            var eventCreator = await _userManager.FindByIdAsync(comment.Event.CreatorId.ToString());
            var eventCreatorRoles = await _userManager.GetRolesAsync(eventCreator);
            
            // Role hierarchy
            var hierarchy = new Dictionary<string, int> {
                ["User"] = 0,
                ["Organizer"] = 1,
                ["Admin"] = 2,
                ["SeniorAdmin"] = 3,
                ["Owner"] = 4
            };
            
            int currentMax = currentRoles.Where(r => hierarchy.ContainsKey(r)).DefaultIfEmpty("User").Max(r => hierarchy[r]);
            int eventCreatorMax = eventCreatorRoles.Where(r => hierarchy.ContainsKey(r)).DefaultIfEmpty("User").Max(r => hierarchy[r]);
            
            // Owner can delete any comment
            if (currentRoles.Contains("Owner"))
            {
                // Allow deletion
            }
            // SeniorAdmin can delete comments in events below their rank (except Owner events)
            else if (currentRoles.Contains("SeniorAdmin"))
            {
                if (eventCreatorMax >= 4) // Owner event
                {
                    return new ContentResult
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        Content = "Cannot delete comments in Owner events",
                        ContentType = "text/plain"
                    };
                }
            }
            // Admin can delete comments only in their own events
            else if (currentRoles.Contains("Admin"))
            {
                if (currentUser.Id != comment.Event.CreatorId)
                {
                    return new ContentResult
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        Content = "Admins can only delete comments in their own events",
                        ContentType = "text/plain"
                    };
                }
            }
            // Regular users can only delete their own comments
            else
            {
                if (comment.UserId != userId)
                {
                    return Forbid();
                }
            }
            
            _db.EventComments.Remove(comment);
            await _db.SaveChangesAsync();
            
            // Log the comment deletion
            var userAgent = GetUserAgent();
            var details = $"Deleted comment {commentId} from event '{comment.Event.Title}' (ID: {comment.Event.Id})";
            await _activityLogService.LogCommentActivityAsync(userId, "Delete", commentId, details, userAgent);
            
            return Ok();
        }

        // 2.6 Delete by any admin
        // DELETE /api/comments/admin/{commentId}
        [HttpDelete("admin/{commentId}"), Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> DeleteAny(int commentId)
        {
            var comment = await _db.EventComments.Include(x => x.Event).FirstOrDefaultAsync(x => x.Id == commentId);
            if (comment == null) return NotFound();
            
            // Get current user and their roles
            var currentUser = await _userManager.GetUserAsync(User);
            var currentRoles = await _userManager.GetRolesAsync(currentUser);
            
            // Get comment author and their roles
            var author = await _userManager.FindByIdAsync(comment.UserId.ToString());
            var authorRoles = await _userManager.GetRolesAsync(author);
            
            // Get event creator and their roles
            var eventCreator = await _userManager.FindByIdAsync(comment.Event.CreatorId.ToString());
            var eventCreatorRoles = await _userManager.GetRolesAsync(eventCreator);
            
            // Role hierarchy
            var hierarchy = new Dictionary<string, int> {
                ["User"] = 0,
                ["Organizer"] = 1,
                ["Admin"] = 2,
                ["SeniorAdmin"] = 3,
                ["Owner"] = 4
            };
            
            int currentMax = currentRoles.Where(r => hierarchy.ContainsKey(r)).DefaultIfEmpty("User").Max(r => hierarchy[r]);
            int authorMax = authorRoles.Where(r => hierarchy.ContainsKey(r)).DefaultIfEmpty("User").Max(r => hierarchy[r]);
            int eventCreatorMax = eventCreatorRoles.Where(r => hierarchy.ContainsKey(r)).DefaultIfEmpty("User").Max(r => hierarchy[r]);
            
            // Owner can delete any comment
            if (currentRoles.Contains("Owner"))
            {
                // Allow deletion
            }
            // SeniorAdmin can delete comments in events below their rank (except Owner events)
            else if (currentRoles.Contains("SeniorAdmin"))
            {
                if (eventCreatorMax >= 4) // Owner event
                {
                    return new ContentResult
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        Content = "Cannot delete comments in Owner events",
                        ContentType = "text/plain"
                    };
                }
            }
            // Admin can delete comments only in their own events and events below their rank
            else if (currentRoles.Contains("Admin"))
            {
                // Check if it's their own event
                if (currentUser.Id != comment.Event.CreatorId)
                {
                    return new ContentResult
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        Content = "Admins can only delete comments in their own events",
                        ContentType = "text/plain"
                    };
                }
            }
            else
            {
                return Forbid();
            }
            
            var adminUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            _db.EventComments.Remove(comment);
            await _db.SaveChangesAsync();
            
            // Log the admin comment deletion
            var userAgent = GetUserAgent();
            var details = $"Admin deleted comment {commentId} from event '{comment.Event.Title}' (ID: {comment.Event.Id})";
            await _activityLogService.LogCommentActivityAsync(adminUserId, "AdminDelete", commentId, details, userAgent);
            
            return Ok();
        }

        [HttpPatch("pin/{commentId}"), Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> Pin(int commentId, [FromQuery] bool pinned)
        {
            var comment = await _db.EventComments
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null) return NotFound();

            // Get current user and their roles
            var currentUser = await _userManager.GetUserAsync(User);
            var currentRoles = await _userManager.GetRolesAsync(currentUser);
            
            // Get comment author and their roles
            var author = await _userManager.FindByIdAsync(comment.UserId.ToString());
            var authorRoles = await _userManager.GetRolesAsync(author);
            
            // Get event creator and their roles
            var eventCreator = await _userManager.FindByIdAsync(comment.Event.CreatorId.ToString());
            var eventCreatorRoles = await _userManager.GetRolesAsync(eventCreator);
            
            // Cannot pin your own comment
            if (currentUser.Id == comment.UserId)
            {
                return new ContentResult
                {
                    StatusCode = StatusCodes.Status403Forbidden,
                    Content = "Cannot pin your own comment",
                    ContentType = "text/plain"
                };
            }
            
            // Role hierarchy
            var hierarchy = new Dictionary<string, int> {
                ["User"] = 0,
                ["Organizer"] = 1,
                ["Admin"] = 2,
                ["SeniorAdmin"] = 3,
                ["Owner"] = 4
            };
            
            int currentMax = currentRoles.Where(r => hierarchy.ContainsKey(r)).DefaultIfEmpty("User").Max(r => hierarchy[r]);
            int authorMax = authorRoles.Where(r => hierarchy.ContainsKey(r)).DefaultIfEmpty("User").Max(r => hierarchy[r]);
            int eventCreatorMax = eventCreatorRoles.Where(r => hierarchy.ContainsKey(r)).DefaultIfEmpty("User").Max(r => hierarchy[r]);
            
            // Owner can pin any comment
            if (currentRoles.Contains("Owner"))
            {
                // Allow pinning
            }
            // SeniorAdmin can pin comments in events below their rank (except Owner events)
            else if (currentRoles.Contains("SeniorAdmin"))
            {
                if (eventCreatorMax >= 4) // Owner event
                {
                    return new ContentResult
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        Content = "Cannot pin comments in Owner events",
                        ContentType = "text/plain"
                    };
                }
            }
            // Admin can pin comments only in their own events
            else if (currentRoles.Contains("Admin"))
            {
                if (currentUser.Id != comment.Event.CreatorId)
                {
                    return new ContentResult
                    {
                        StatusCode = StatusCodes.Status403Forbidden,
                        Content = "Admins can only pin comments in their own events",
                        ContentType = "text/plain"
                    };
                }
            }
            else
            {
                return Forbid();
            }

            comment.IsPinned = pinned;
            comment.PinnedAt = pinned ? DateTime.UtcNow : (DateTime?)null;
            await _db.SaveChangesAsync();
            return Ok(new { comment.Id, comment.IsPinned, comment.PinnedAt });
        }
    }
}
