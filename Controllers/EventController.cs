using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EventHub.Data;
using EventHub.Models;
using EventHub.Models.DTOs;
using EventHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;

namespace EventHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class EventController : ControllerBase
    {
        private readonly EventHubDbContext _db;
        private readonly ILogger<EventController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IActivityLogService _activityLogService;
        
        public EventController(EventHubDbContext db, ILogger<EventController> logger, UserManager<User> userManager, IActivityLogService activityLogService)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
            _activityLogService = activityLogService;
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }

        private async Task<bool> IsUserOrganizer(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;
            
            var roles = await _userManager.GetRolesAsync(user);
            
            // Define role hierarchy (higher roles include lower roles)
            if (roles.Contains("Owner")) return true;
            if (roles.Contains("SeniorAdmin")) return true;
            if (roles.Contains("Admin")) return true;
            if (roles.Contains("Organizer")) return true;
            
            return false;
        }
        
        private static readonly Dictionary<string,int> RolePriority = new()
        {
            ["Owner"] = 4,
            ["SeniorAdmin"] = 3,
            ["Admin"] = 2,
            ["Organizer"] = 1,
            ["User"] = 0
        };

        private async Task<EventDto> CreateEventDtoWithRoles(Event e, int? userId)
        {
            var creatorRoles = await _userManager.GetRolesAsync(e.Creator!);
            
            return new EventDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                Category = e.Category,
                Location = e.Location,
                MaxParticipants = e.MaxParticipants,
                OrganizerEmail = e.Creator!.Email,
                OrganizerName = e.Creator!.Name,
                CreatorId = e.CreatorId,
                CreatorRoles = creatorRoles.ToArray(),
                IsFavorite = userId.HasValue && _db.FavoriteEvents.Any(f => f.EventId == e.Id && f.UserId == userId.Value),
                IsPlanned = userId.HasValue && _db.PlannedEvents.Any(p => p.EventId == e.Id && p.UserId == userId.Value),
                CommentsCount = e.EventComments.Count
            };
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<EventDto>>> GetEvents(
            [FromQuery] string? searchTerm,
            [FromQuery] string? category,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortBy = "StartDate",
            [FromQuery] string order = "asc")
        {
            var userId = GetCurrentUserId();
            IQueryable<Event> query = _db.Events
                .Include(e => e.Creator)
                .Include(e => e.EventComments);

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(e => 
                    e.Title.Contains(searchTerm) || 
                    e.Description.Contains(searchTerm) ||
                    e.Location.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(e => e.Category == category);
            }
            
            if (startDate.HasValue && startDate.Value.Kind == DateTimeKind.Unspecified)
                startDate = DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc);

            if (endDate.HasValue && endDate.Value.Kind == DateTimeKind.Unspecified)
                endDate = DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc);

            if (startDate.HasValue)
            {
                var startUtc = DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc);
                query = query.Where(e => e.StartDate >= startUtc);
            }

            if (endDate.HasValue)
            {
                var endUtc = DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc);
                // Find events that start before or on the end date (events that occur during the period)
                query = query.Where(e => e.StartDate <= endUtc);
            }


            // Apply sorting
            switch (sortBy)
            {
                case "Title":
                    query = order == "desc"
                        ? query.OrderByDescending(e => e.Title)
                        : query.OrderBy(e => e.Title);
                    break;
                case "EndDate":
                    query = order == "desc"
                        ? query.OrderByDescending(e => e.EndDate)
                        : query.OrderBy(e => e.EndDate);
                    break;
                case "Category":
                    query = order == "desc"
                        ? query.OrderByDescending(e => e.Category)
                        : query.OrderBy(e => e.Category);
                    break;
                default: // StartDate
                    query = order == "desc"
                        ? query.OrderByDescending(e => e.StartDate)
                        : query.OrderBy(e => e.StartDate);
                    break;
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Apply pagination
            var events = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Create DTOs with roles
            var eventDtos = new List<EventDto>();
            foreach (var e in events)
            {
                eventDtos.Add(await CreateEventDtoWithRoles(e, userId));
            }

            return Ok(new PaginatedResponse<EventDto>
            {
                Items = eventDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            });
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EventDto>> GetEvent(int id)
        {
            var userId = GetCurrentUserId();

            var evt = await _db.Events
                .Include(e => e.Creator)
                .Include(e => e.EventComments)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (evt == null)
            {
                return NotFound(new { message = "Event not found" });
            }

            var eventDto = await CreateEventDtoWithRoles(evt, userId);
            return Ok(eventDto);
        }


        /// <summary>
        /// GET: api/Event/category/{category}
        /// Search events by category
        /// </summary>
        [HttpGet("category/{category}")]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetByCategory(
            string category,
            [FromQuery] string sortBy = "StartDate",
            [FromQuery] string order = "asc")
        {
            var userId = GetCurrentUserId();
            IQueryable<Event> query = _db.Events
                .Include(e => e.Creator)
                .Include(e => e.EventComments)
                .Where(e => e.Category == category);

            // Sorting similarly
            switch (sortBy)
            {
                case "Title":
                    query = order == "desc"
                        ? query.OrderByDescending(e => e.Title)
                        : query.OrderBy(e => e.Title);
                    break;
                case "EndDate":
                    query = order == "desc"
                        ? query.OrderByDescending(e => e.EndDate)
                        : query.OrderBy(e => e.EndDate);
                    break;
                default: // StartDate and other fields
                    query = order == "desc"
                        ? query.OrderByDescending(e => e.StartDate)
                        : query.OrderBy(e => e.StartDate);
                    break;
            }

            var events = await query.ToListAsync();

            // Create DTOs with roles
            var eventDtos = new List<EventDto>();
            foreach (var e in events)
            {
                eventDtos.Add(await CreateEventDtoWithRoles(e, userId));
            }

            return Ok(eventDtos);
        }

        // ADMIN COMMANDS: 

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<EventDto>> CreateEvent(EventCreateDto eventDto)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "Authentication required" });

            // Check if user is muted
            var muteEntry = await _db.UserMuteEntries.FindAsync(userId.Value);
            
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
            
            // If user is currently muted, forbid event creation
            if (muteEntry?.IsMuted == true)
            {
                return Forbid("You are currently muted and cannot create events.");
            }

            if (!await IsUserOrganizer(userId.Value))
            {
                _logger.LogWarning($"User {userId} unauthorized to create event");
                return Forbid();
            }

            // Validate dates
            var today = DateTime.UtcNow.Date;
            var startDate = DateTime.SpecifyKind(eventDto.StartDate, DateTimeKind.Utc).Date;
            var endDate = DateTime.SpecifyKind(eventDto.EndDate, DateTimeKind.Utc).Date;

            if (startDate < today)
            {
                return BadRequest(new { message = "Start date cannot be in the past. Please select a future date." });
            }

            if (endDate < startDate)
            {
                return BadRequest(new { message = "End date cannot be earlier than start date." });
            }

            var evt = new Event
            {
                Title = eventDto.Title,
                Description = eventDto.Description,
                StartDate = DateTime.SpecifyKind(eventDto.StartDate, DateTimeKind.Utc),
                EndDate = DateTime.SpecifyKind(eventDto.EndDate, DateTimeKind.Utc),
                Category = eventDto.Category,
                Location = eventDto.Location,
                MaxParticipants = eventDto.MaxParticipants,
                CreatorId = userId.Value
            };

            _db.Events.Add(evt);
            await _db.SaveChangesAsync();

            // Log event creation
            await _activityLogService.LogEventActivityAsync(
                userId.Value,
                "EVENT_CREATED",
                evt.Id,
                $"Created event '{evt.Title}' in category '{evt.Category}'",
                HttpContext.Request.Headers["User-Agent"].ToString()
            );

            var result = new EventDto(evt)
            {
                IsFavorite = false,
                IsPlanned = false
            };

            return CreatedAtAction(nameof(GetEvent), new { id = evt.Id }, result);
        }

        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEvent(int id, EventCreateDto eventDto)
        {

            var maybeUserId = GetCurrentUserId();
            if (!maybeUserId.HasValue) 
                return Unauthorized(new { message = "Authentication required" });

            var userId = maybeUserId.Value;
            
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
            
            // If user is currently muted, forbid event updates
            if (muteEntry?.IsMuted == true)
            {
                return Forbid("You are currently muted and cannot update events.");
            }
            var evt = await _db.Events.FindAsync(id);
            if (evt == null)
                return NotFound(new { message = "Event not found" });

            // --- NEW: check creator role vs current user role ---
            var creator = await _userManager.FindByIdAsync(evt.CreatorId.ToString());
            var creatorRoles = await _userManager.GetRolesAsync(creator);
            var creatorMax = creatorRoles
                .Select(r => RolePriority.GetValueOrDefault(r, 0))
                .DefaultIfEmpty(0)
                .Max();

            var current = await _userManager.GetUserAsync(User);
            var currentRoles = await _userManager.GetRolesAsync(current);
            var currentMax = currentRoles
                .Select(r => RolePriority.GetValueOrDefault(r, 0))
                .DefaultIfEmpty(0)
                .Max();

            // If our priority is lower than creator's — forbid
            if (currentMax < creatorMax)
            {
                _logger.LogWarning(
                "User {UserId} (prio {Curr}) cannot edit event {EventId} created by {CreatorId} (prio {Cre})",
                userId, currentMax, id, evt.CreatorId, creatorMax);
                return Forbid();
            }
            // -----------------------------------------------------------------------

            // old checks: author and organizer
            if (evt.CreatorId != userId && !await IsUserOrganizer(userId))
            {
                _logger.LogWarning($"User {userId} unauthorized to update event {id}");
                return Forbid();
            }

            // Validate dates
            var today = DateTime.UtcNow.Date;
            var startDate = DateTime.SpecifyKind(eventDto.StartDate, DateTimeKind.Utc).Date;
            var endDate = DateTime.SpecifyKind(eventDto.EndDate, DateTimeKind.Utc).Date;

            if (startDate < today)
            {
                return BadRequest(new { message = "Start date cannot be in the past. Please select a future date." });
            }

            if (endDate < startDate)
            {
                return BadRequest(new { message = "End date cannot be earlier than start date." });
            }

            // apply changes...
            evt.Title           = eventDto.Title;
            evt.Description     = eventDto.Description;
            evt.StartDate       = DateTime.SpecifyKind(eventDto.StartDate, DateTimeKind.Utc);
            evt.EndDate         = DateTime.SpecifyKind(eventDto.EndDate, DateTimeKind.Utc);
            evt.Category        = eventDto.Category;
            evt.Location        = eventDto.Location;
            evt.MaxParticipants = eventDto.MaxParticipants;

            try
            {
                await _db.SaveChangesAsync();
                
                // Log event update
                await _activityLogService.LogEventActivityAsync(
                    userId,
                    "EVENT_UPDATED",
                    evt.Id,
                    $"Updated event '{evt.Title}' in category '{evt.Category}'",
                    HttpContext.Request.Headers["User-Agent"].ToString()
                );
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(id))
                    return NotFound(new { message = "Event not found" });
                throw;
            }

            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var maybeUserId = GetCurrentUserId();
            if (!maybeUserId.HasValue) 
                return Unauthorized(new { message = "Authentication required" });

            var userId = maybeUserId.Value;
            
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
            
            // If user is currently muted, forbid event deletion
            if (muteEntry?.IsMuted == true)
            {
                return Forbid("You are currently muted and cannot delete events.");
            }
            var evt = await _db.Events.FindAsync(id);
            if (evt == null)
                return NotFound(new { message = "Event not found" });

            // Similarly: don't allow deletion if priority is lower than creator's
            var creator = await _userManager.FindByIdAsync(evt.CreatorId.ToString());
            var creatorRoles = await _userManager.GetRolesAsync(creator);
            var creatorMax = creatorRoles
                .Select(r => RolePriority.GetValueOrDefault(r, 0))
                .DefaultIfEmpty(0)
                .Max();

            var current = await _userManager.GetUserAsync(User);
            var currentRoles = await _userManager.GetRolesAsync(current);
            var currentMax = currentRoles
                .Select(r => RolePriority.GetValueOrDefault(r, 0))
                .DefaultIfEmpty(0)
                .Max();

            if (currentMax < creatorMax)
            {
                _logger.LogWarning(
                "User {UserId} (prio {Curr}) cannot delete event {EventId} created by {CreatorId} (prio {Cre})",
                userId, currentMax, id, evt.CreatorId, creatorMax);
                return Forbid();
            }

            if (evt.CreatorId != userId && !await IsUserOrganizer(userId))
            {
                _logger.LogWarning($"User {userId} unauthorized to delete event {id}");
                return Forbid();
            }

            // Log event deletion
            await _activityLogService.LogEventActivityAsync(
                userId,
                "EVENT_DELETED",
                evt.Id,
                $"Deleted event '{evt.Title}' in category '{evt.Category}'",
                HttpContext.Request.Headers["User-Agent"].ToString()
            );

            _db.Events.Remove(evt);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("user/{userId:int}")]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetEventsByUser(int userId)
        {
            var currentUserId = GetCurrentUserId();
            var events = await _db.Events
                .Include(e => e.Creator)
                .Include(e => e.EventComments)
                .Where(e => e.CreatorId == userId)
                .Select(e => new EventDto(e)
                {
                    IsFavorite = currentUserId.HasValue && _db.FavoriteEvents.Any(f => f.EventId == e.Id && f.UserId == currentUserId.Value),
                    IsPlanned = currentUserId.HasValue && _db.PlannedEvents.Any(p => p.EventId == e.Id && p.UserId == currentUserId.Value)
                })
                .ToListAsync();

            return Ok(events);
        }

        private bool EventExists(int id) => _db.Events.Any(e => e.Id == id);
    }
}
