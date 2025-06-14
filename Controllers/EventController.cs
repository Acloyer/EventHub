using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventHub.Data;
using EventHub.Models;
using EventHub.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace EventHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EventController> _logger;

        public EventController(ApplicationDbContext context, ILogger<EventController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private int? GetCurrentUserId()
        {
            try
            {
                if (User.Identity?.IsAuthenticated != true)
                    return null;

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                return userIdClaim != null ? int.Parse(userIdClaim.Value) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user ID");
                return null;
            }
        }

        private async Task<bool> IsUserOrganizer(int userId)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            return user?.UserRoles.Any(ur => ur.Role?.Name == "Organizer" 
                                          || ur.Role?.Name == "Admin" 
                                          || ur.Role?.Name == "SeniorAdmin" 
                                          || ur.Role?.Name == "Owner") ?? false;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetEvents()
        {
            var userId = GetCurrentUserId();

            var events = await _context.Events
                .Include(e => e.Creator)
                    .ThenInclude(u => u!.UserRoles)
                        .ThenInclude(ur => ur.Role)
                .Select(e => new EventDto
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
                    CreatorId = e.CreatorId,
                    IsFavorite = userId.HasValue && _context.FavoriteEvents.Any(f => f.EventId == e.Id && f.UserId == userId.Value),
                    IsPlanned = userId.HasValue && _context.PlannedEvents.Any(p => p.EventId == e.Id && p.UserId == userId.Value)
                })
                .ToListAsync();

            return events;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EventDto>> GetEvent(int id)
        {
            var userId = GetCurrentUserId();

            var @event = await _context.Events
                .Include(e => e.Creator)
                    .ThenInclude(u => u!.UserRoles)
                        .ThenInclude(ur => ur.Role)
                .Where(e => e.Id == id)
                .Select(e => new EventDto
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
                    CreatorId = e.CreatorId,
                    IsFavorite = userId.HasValue && _context.FavoriteEvents.Any(f => f.EventId == e.Id && f.UserId == userId.Value),
                    IsPlanned = userId.HasValue && _context.PlannedEvents.Any(p => p.EventId == e.Id && p.UserId == userId.Value)
                })
                .FirstOrDefaultAsync();

            if (@event == null)
            {
                return NotFound(new { message = "Event not found" });
            }

            return @event;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<EventDto>> CreateEvent(EventDto eventDto)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                _logger.LogWarning("Attempt to create event without authentication");
                return Unauthorized(new { message = "Authentication required" });
            }

            if (!await IsUserOrganizer(userId.Value))
            {
                _logger.LogWarning($"User {userId} attempted to create event without proper role");
                return StatusCode(403, new { message = "Only organizers and administrators can create events" });
            }

            var @event = new Event
            {
                Title = eventDto.Title,
                Description = eventDto.Description,
                StartDate = eventDto.StartDate,
                EndDate = eventDto.EndDate,
                Category = eventDto.Category,
                Location = eventDto.Location,
                MaxParticipants = eventDto.MaxParticipants,
                CreatorId = userId.Value
            };

            _context.Events.Add(@event);
            await _context.SaveChangesAsync();

            var createdEvent = await GetEvent(@event.Id);
            return CreatedAtAction(nameof(GetEvent), new { id = @event.Id }, createdEvent.Value);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(int id, EventDto eventDto)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                _logger.LogWarning("Attempt to update event without authentication");
                return Unauthorized(new { message = "Authentication required" });
            }

            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound(new { message = "Event not found" });
            }

            if (@event.CreatorId != userId && !User.IsInRole("Admin"))
            {
                _logger.LogWarning($"User {userId} attempted to update event {id} without proper permissions");
                return StatusCode(403, new { message = "Only the event creator or administrators can update this event" });
            }

            @event.Title = eventDto.Title;
            @event.Description = eventDto.Description;
            @event.StartDate = eventDto.StartDate;
            @event.EndDate = eventDto.EndDate;
            @event.Category = eventDto.Category;
            @event.Location = eventDto.Location;
            @event.MaxParticipants = eventDto.MaxParticipants;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, $"Concurrency error updating event {id}");
                if (!EventExists(id))
                    return NotFound(new { message = "Event not found" });
                throw;
            }

            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                _logger.LogWarning("Attempt to delete event without authentication");
                return Unauthorized(new { message = "Authentication required" });
            }

            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound(new { message = "Event not found" });
            }

            if (@event.CreatorId != userId && !User.IsInRole("Admin"))
            {
                _logger.LogWarning($"User {userId} attempted to delete event {id} without proper permissions");
                return StatusCode(403, new { message = "Only the event creator or administrators can delete this event" });
            }

            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.Id == id);
        }
    }
}
