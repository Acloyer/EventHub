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
        
        public EventController(EventHubDbContext db, ILogger<EventController> logger, UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }

        private async Task<bool> IsUserOrganizer(int userId)
        {
            // через Identity
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;
            var roles = await _userManager.GetRolesAsync(user);
            return roles.Any(r =>
                r == "Organizer" ||
                r == "Admin" ||
                r == "SeniorAdmin" ||
                r == "Owner");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetEvents(
            [FromQuery] string sortBy = "StartDate",
            [FromQuery] string order = "asc")
        {
            var userId = GetCurrentUserId();
            IQueryable<Event> query = _db.Events.Include(e => e.Creator);

            // Сортировка
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

            var events = await query
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
                    OrganizerName = e.Creator!.Name,
                    CreatorId = e.CreatorId,
                    IsFavorite = userId.HasValue && _db.FavoriteEvents.Any(f => f.EventId == e.Id && f.UserId == userId.Value),
                    IsPlanned = userId.HasValue && _db.PlannedEvents.Any(p => p.EventId == e.Id && p.UserId == userId.Value)
                })
                .ToListAsync();

            return Ok(events);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EventDto>> GetEvent(int id)
        {
            var userId = GetCurrentUserId();

            var evt = await _db.Events
                .Include(e => e.Creator)
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
                    OrganizerName = e.Creator!.Name,
                    CreatorId = e.CreatorId,
                    IsFavorite = userId.HasValue && _db.FavoriteEvents.Any(f => f.EventId == e.Id && f.UserId == userId.Value),
                    IsPlanned = userId.HasValue && _db.PlannedEvents.Any(p => p.EventId == e.Id && p.UserId == userId.Value)
                })
                .FirstOrDefaultAsync();

            if (evt == null)
            {
                return NotFound(new { message = "Event not found" });
            }

            return Ok(evt);
        }


        /// <summary>
        /// GET: api/Event/category/{category}
        /// Поиск событий по категории
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
                .Where(e => e.Category == category);

            // Сортировка аналогично
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
                default: // StartDate и другие поля
                    query = order == "desc"
                        ? query.OrderByDescending(e => e.StartDate)
                        : query.OrderBy(e => e.StartDate);
                    break;
            }

            var events = await query
                .Select(e => new EventDto(e)
                {
                    IsFavorite = userId.HasValue && _db.FavoriteEvents.Any(f => f.EventId == e.Id && f.UserId == userId.Value),
                    IsPlanned = userId.HasValue && _db.PlannedEvents.Any(p => p.EventId == e.Id && p.UserId == userId.Value)
                })
                .ToListAsync();

            return Ok(events);
        }

        // ADMIN COMMANDS: 

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<EventDto>> CreateEvent(EventCreateDto eventDto)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "Authentication required" });

            if (!await IsUserOrganizer(userId.Value))
            {
                _logger.LogWarning($"User {userId} unauthorized to create event");
                return Forbid();
            }

            var evt = new Event
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

            _db.Events.Add(evt);
            await _db.SaveChangesAsync();

            var created = await _db.Events.FindAsync(evt.Id);
            return CreatedAtAction(nameof(GetEvent), new { id = evt.Id }, new EventDto(created!));
        }

        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEvent(int id, EventDto eventDto)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "Authentication required" });

            var evt = await _db.Events.FindAsync(id);
            if (evt == null)
                return NotFound(new { message = "Event not found" });

            if (evt.CreatorId != userId.Value && !User.IsInRole("Admin"))
                return Forbid();

            evt.Title = eventDto.Title;
            evt.Description = eventDto.Description;
            evt.StartDate = eventDto.StartDate;
            evt.EndDate = eventDto.EndDate;
            evt.Category = eventDto.Category;
            evt.Location = eventDto.Location;
            evt.MaxParticipants = eventDto.MaxParticipants;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_db.Events.Any(e => e.Id == id))
                    return NotFound(new { message = "Event not found" });
                throw;
            }

            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "Authentication required" });

            var evt = await _db.Events.FindAsync(id);
            if (evt == null)
                return NotFound(new { message = "Event not found" });

            if (evt.CreatorId != userId.Value && !User.IsInRole("Admin"))
                return Forbid();

            _db.Events.Remove(evt);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("user/{userId:int}")]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetEventsByUser(int userId)
        {
            var events = await _db.Events
                .Where(e => e.CreatorId == userId)
                .Select(e => new EventDto(e))
                .ToListAsync();

            return Ok(events);
        }

        private bool EventExists(int id) => _db.Events.Any(e => e.Id == id);
    }
}
