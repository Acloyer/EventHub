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
<<<<<<< HEAD
        private readonly IActivityLogService _activityLogService;
        
        public EventController(EventHubDbContext db, ILogger<EventController> logger, UserManager<User> userManager, IActivityLogService activityLogService)
=======
<<<<<<< HEAD
        private readonly IActivityLogService _activityLogService;
        
        public EventController(EventHubDbContext db, ILogger<EventController> logger, UserManager<User> userManager, IActivityLogService activityLogService)
=======
<<<<<<< HEAD
        private readonly IActivityLogService _activityLogService;
        
        public EventController(EventHubDbContext db, ILogger<EventController> logger, UserManager<User> userManager, IActivityLogService activityLogService)
=======
        
        public EventController(EventHubDbContext db, ILogger<EventController> logger, UserManager<User> userManager)
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
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
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
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
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
=======
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
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
            [FromQuery] string sortBy = "StartDate",
            [FromQuery] string order = "asc")
        {
            var userId = GetCurrentUserId();
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
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
<<<<<<< HEAD
                // Find events that start before or on the end date (events that occur during the period)
                query = query.Where(e => e.StartDate <= endUtc);
=======
<<<<<<< HEAD
                // Find events that start before or on the end date (events that occur during the period)
                query = query.Where(e => e.StartDate <= endUtc);
=======
                query = query.Where(e => e.EndDate <= endUtc);
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
            }


            // Apply sorting
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
=======
            IQueryable<Event> query = _db.Events.Include(e => e.Creator);

            // Сортировка
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
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

<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
            // Get total count for pagination
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Apply pagination
            var events = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
<<<<<<< HEAD
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
=======
<<<<<<< HEAD
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
=======
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
=======
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
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EventDto>> GetEvent(int id)
        {
            var userId = GetCurrentUserId();

            var evt = await _db.Events
                .Include(e => e.Creator)
<<<<<<< HEAD
                .Include(e => e.EventComments)
                .FirstOrDefaultAsync(e => e.Id == id);
=======
<<<<<<< HEAD
                .Include(e => e.EventComments)
                .FirstOrDefaultAsync(e => e.Id == id);
=======
<<<<<<< HEAD
                .Include(e => e.EventComments)
                .FirstOrDefaultAsync(e => e.Id == id);
=======
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
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9

            if (evt == null)
            {
                return NotFound(new { message = "Event not found" });
            }

<<<<<<< HEAD
            var eventDto = await CreateEventDtoWithRoles(evt, userId);
            return Ok(eventDto);
=======
<<<<<<< HEAD
            var eventDto = await CreateEventDtoWithRoles(evt, userId);
            return Ok(eventDto);
=======
<<<<<<< HEAD
            var eventDto = await CreateEventDtoWithRoles(evt, userId);
            return Ok(eventDto);
=======
            return Ok(evt);
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
        }


        /// <summary>
        /// GET: api/Event/category/{category}
<<<<<<< HEAD
        /// Search events by category
=======
<<<<<<< HEAD
        /// Search events by category
=======
<<<<<<< HEAD
        /// Search events by category
=======
        /// Поиск событий по категории
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
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
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
                .Include(e => e.EventComments)
                .Where(e => e.Category == category);

            // Sorting similarly
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
=======
                .Where(e => e.Category == category);

            // Сортировка аналогично
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
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
<<<<<<< HEAD
                default: // StartDate and other fields
=======
<<<<<<< HEAD
                default: // StartDate and other fields
=======
<<<<<<< HEAD
                default: // StartDate and other fields
=======
                default: // StartDate и другие поля
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
                    query = order == "desc"
                        ? query.OrderByDescending(e => e.StartDate)
                        : query.OrderBy(e => e.StartDate);
                    break;
            }

<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
            var events = await query.ToListAsync();

            // Create DTOs with roles
            var eventDtos = new List<EventDto>();
            foreach (var e in events)
            {
                eventDtos.Add(await CreateEventDtoWithRoles(e, userId));
            }

            return Ok(eventDtos);
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
=======
            var events = await query
                .Select(e => new EventDto(e)
                {
                    IsFavorite = userId.HasValue && _db.FavoriteEvents.Any(f => f.EventId == e.Id && f.UserId == userId.Value),
                    IsPlanned = userId.HasValue && _db.PlannedEvents.Any(p => p.EventId == e.Id && p.UserId == userId.Value)
                })
                .ToListAsync();

            return Ok(events);
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
        }

        // ADMIN COMMANDS: 

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<EventDto>> CreateEvent(EventCreateDto eventDto)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "Authentication required" });
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9

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
<<<<<<< HEAD
            }
            
            // If user is currently muted, forbid event creation
            if (muteEntry?.IsMuted == true)
            {
                return Forbid("You are currently muted and cannot create events.");
=======
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
            }
            
            // If user is currently muted, forbid event creation
            if (muteEntry?.IsMuted == true)
            {
                return Forbid("You are currently muted and cannot create events.");
            }
=======
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c

            if (!await IsUserOrganizer(userId.Value))
            {
                _logger.LogWarning($"User {userId} unauthorized to create event");
                return Forbid();
            }

<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
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

<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
=======
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
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

<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
            // Log event creation
            await _activityLogService.LogEventActivityAsync(
                userId.Value,
                "EVENT_CREATED",
                evt.Id,
                $"Created event '{evt.Title}' in category '{evt.Category}'",
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
                HttpContext.Request.Headers["User-Agent"].ToString()
            );

            var result = new EventDto(evt)
            {
                IsFavorite = false,
                IsPlanned = false
            };

            return CreatedAtAction(nameof(GetEvent), new { id = evt.Id }, result);
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
=======
            var created = await _db.Events.FindAsync(evt.Id);
            return CreatedAtAction(nameof(GetEvent), new { id = evt.Id }, new EventDto(created!));
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
        }

        [Authorize]
        [HttpPut("{id:int}")]
<<<<<<< HEAD
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
=======
        public async Task<IActionResult> UpdateEvent(int id, EventDto eventDto)
        {
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
            if (id != eventDto.Id)
                return BadRequest(new { message = "ID mismatch" });

            var maybeUserId = GetCurrentUserId();
            if (!maybeUserId.HasValue) 
<<<<<<< HEAD
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
=======
                return Unauthorized(new { message = "Authentication required" });

            var userId = maybeUserId.Value;
=======
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "Authentication required" });

>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
            var evt = await _db.Events.FindAsync(id);
            if (evt == null)
                return NotFound(new { message = "Event not found" });

<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
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
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
=======
            if (evt.CreatorId != userId.Value && !User.IsInRole("Admin"))
                return Forbid();

            evt.Title = eventDto.Title;
            evt.Description = eventDto.Description;
            evt.StartDate = eventDto.StartDate;
            evt.EndDate = eventDto.EndDate;
            evt.Category = eventDto.Category;
            evt.Location = eventDto.Location;
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
            evt.MaxParticipants = eventDto.MaxParticipants;

            try
            {
                await _db.SaveChangesAsync();
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
                
                // Log event update
                await _activityLogService.LogEventActivityAsync(
                    userId,
                    "EVENT_UPDATED",
                    evt.Id,
                    $"Updated event '{evt.Title}' in category '{evt.Category}'",
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
                    HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
                    HttpContext.Request.Headers["User-Agent"].ToString()
                );
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(id))
=======
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_db.Events.Any(e => e.Id == id))
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
                    return NotFound(new { message = "Event not found" });
                throw;
            }

            return NoContent();
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
<<<<<<< HEAD
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
=======
<<<<<<< HEAD
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
=======
<<<<<<< HEAD
            var maybeUserId = GetCurrentUserId();
            if (!maybeUserId.HasValue)
                return Unauthorized(new { message = "Authentication required" });

            var userId = maybeUserId.Value;
=======
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "Authentication required" });

>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
            var evt = await _db.Events.FindAsync(id);
            if (evt == null)
                return NotFound(new { message = "Event not found" });

<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
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
<<<<<<< HEAD
                HttpContext.Request.Headers["User-Agent"].ToString()
            );

=======
<<<<<<< HEAD
                HttpContext.Request.Headers["User-Agent"].ToString()
            );

=======
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                HttpContext.Request.Headers["User-Agent"].ToString()
            );

=======
            if (evt.CreatorId != userId.Value && !User.IsInRole("Admin"))
                return Forbid();

>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
            _db.Events.Remove(evt);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("user/{userId:int}")]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetEventsByUser(int userId)
        {
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
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
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
=======
            var events = await _db.Events
                .Where(e => e.CreatorId == userId)
                .Select(e => new EventDto(e))
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
                .ToListAsync();

            return Ok(events);
        }

        private bool EventExists(int id) => _db.Events.Any(e => e.Id == id);
    }
}
