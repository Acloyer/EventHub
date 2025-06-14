using EventHub.Data;
using EventHub.Models;
using EventHub.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Microsoft.Extensions.Logging;
using EventHub.Services;

namespace EventHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly EventHubDbContext _db;
        private readonly ITelegramBotClient _bot;
        private readonly ILogger<UserController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;

        public UserController(
            EventHubDbContext db,
            ITelegramBotClient bot,
            ILogger<UserController> logger,
            ApplicationDbContext context,
            IUserService userService)
        {
            _db = db;
            _bot = bot;
            _logger = logger;
            _context = context;
            _userService = userService;
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

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(UserRegisterDto registerDto)
        {
            try
            {
                var result = await _userService.Register(registerDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                return StatusCode(500, new { message = "An error occurred during registration" });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(UserLoginDto loginDto)
        {
            try
            {
                var result = await _userService.Login(loginDto);
                if (result == null)
                    return Unauthorized(new { message = "Invalid email or password" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login");
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }

        // GET api/User/Profile
        [Authorize]
        [HttpGet("Profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                _logger.LogWarning("Could not extract user ID from token");
                return Unauthorized(new { message = "Invalid token format" });
            }

            var user = await _db.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                _logger.LogWarning($"User with ID {userId} not found");
                return NotFound(new { message = "User not found" });
            }

            return Ok(new
            {
                user.Id,
                user.Email,
                Roles = user.UserRoles?
                    .Where(ur => ur.Role != null)
                    .Select(ur => ur.Role!.Name)
                    .ToArray() 
                        ?? Array.Empty<string>(),
                user.TelegramId,
                user.IsTelegramVerified,
                user.NotifyBeforeEvent
            });
        }

        // GET api/User/CreatedEvents
        [Authorize]
        [HttpGet("CreatedEvents")]
        public async Task<IActionResult> GetCreatedEvents()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "Authentication required" });
            }

            var eventsData = await _db.Events
                .Include(e => e.Creator)
                    .ThenInclude(u => u!.UserRoles)
                        .ThenInclude(ur => ur.Role)
                .Where(e => e.CreatorId == userId)
                .ToListAsync();

            var eventDtos = eventsData.Select(e => new EventDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                Category = e.Category,
                Location = e.Location,
                MaxParticipants = e.MaxParticipants,
                // Creator = e.Creator != null
                //     ? new UserDto
                //     {
                //         Id = e.Creator.Id,
                //         Email = e.Creator.Email,
                //         Roles = e.Creator.UserRoles
                //             .Where(ur => ur.Role != null)
                //             .Select(ur => ur.Role!.Name)
                //             .ToArray()
                //     }
                //     : null,
                // Здесь мы ВНЕ Select-запроса — контекст уже не конфликтует
                IsFavorite = _db.FavoriteEvents.Any(f => f.EventId == e.Id && f.UserId == userId),
                IsPlanned = _db.PlannedEvents.Any(p => p.EventId == e.Id && p.UserId == userId)
            }).ToList();


            return Ok(eventDtos);
        }

        // GET api/User/Favorites
        [Authorize]
        [HttpGet("Favorites")]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetFavoriteEvents()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                _logger.LogWarning("Attempt to get favorite events without valid user ID");
                return Unauthorized(new { message = "Authentication required" });
            }

            try
            {
                var favoriteEvents = await _context.Events
                    .Include(e => e.Creator)
                        .ThenInclude(u => u!.UserRoles)
                            .ThenInclude(ur => ur.Role)
                    .Where(e => _context.FavoriteEvents
                        .Any(f => f.EventId == e.Id && f.UserId == userId))
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
                        // Creator = new UserDto
                        // {
                        //     Id = e.Creator!.Id,
                        //     Email = e.Creator.Email,
                            // Roles = e.Creator.UserRoles
                            //     .Where(ur => ur.Role != null)
                            //     .Select(ur => ur.Role!.Name)
                            //     .ToArray()
                        // },
                        IsFavorite = true,
                        IsPlanned = _context.PlannedEvents
                            .Any(p => p.EventId == e.Id && p.UserId == userId)
                    })
                    .ToListAsync();

                return Ok(favoriteEvents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting favorite events for user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while retrieving favorite events" });
            }
        }

        // GET api/User/Planned
        [Authorize]
        [HttpGet("Planned")]
        public async Task<ActionResult<IEnumerable<EventDto>>> GetPlannedEvents()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                _logger.LogWarning("Attempt to get planned events without valid user ID");
                return Unauthorized(new { message = "Authentication required" });
            }

            try
            {
                var plannedEvents = await _context.Events
                    .Include(e => e.Creator)
                        .ThenInclude(u => u!.UserRoles)
                            .ThenInclude(ur => ur.Role)
                    .Where(e => _context.PlannedEvents
                        .Any(p => p.EventId == e.Id && p.UserId == userId))
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

                        // 
                        // Creator = e.Creator != null
                        //     ? new UserDto
                        //     {
                        //         Id = e.Creator.Id,
                        //         Email = e.Creator.Email,
                        //         Roles = e.Creator.UserRoles
                        //             .Where(ur => ur.Role != null)
                        //             .Select(ur => ur.Role!.Name)
                        //             .ToArray()
                        //     }
                        //     : null,

                        IsFavorite = _context.FavoriteEvents
                            .Any(f => f.EventId == e.Id && f.UserId == userId),
                        IsPlanned = true
                    })
                    .ToListAsync();

                return Ok(plannedEvents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting planned events for user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while retrieving planned events" });
            }
        }

        // POST api/User/link-telegram/{chatId}
        [HttpPost("link-telegram/{chatId}")]
        public async Task<IActionResult> LinkTelegram(string chatId)
        {
            var userId = GetCurrentUserId();
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            // generate 6-digit code
            var code = new Random().Next(100000, 999999).ToString();
            user.TelegramId = chatId;
            user.TelegramCode = code;
            user.IsTelegramVerified = false;
            await _db.SaveChangesAsync();

            // send code via Bot
            await _bot.SendTextMessageAsync(
                chatId: chatId,
                text: $"🚀 Your verification code: *{code}*\n\n" +
                      "Please enter this code in the app to complete linking.",
                parseMode: ParseMode.Markdown
            );

            return Ok("Verification code sent to Telegram");
        }

        // POST api/User/confirm-telegram
        [HttpPost("confirm-telegram")]
        public async Task<IActionResult> ConfirmTelegram([FromBody] string code)
        {
            var userId = GetCurrentUserId();
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            if (user.TelegramCode != code)
                return BadRequest("Invalid verification code");

            user.IsTelegramVerified = true;
            user.TelegramCode = null;
            await _db.SaveChangesAsync();

            return Ok("Telegram linked successfully");
        }

        // POST api/User/set-notify
        [HttpPost("set-notify")]
        public async Task<IActionResult> SetNotificationPreference([FromBody] bool notify)
        {
            var userId = GetCurrentUserId();
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return NotFound();

            user.NotifyBeforeEvent = notify;
            await _db.SaveChangesAsync();
            return Ok(new { user.NotifyBeforeEvent });
        }

        // Admin endpoints

        // GET api/User/All
        [HttpGet("All")]
        [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _db.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    Roles = u.UserRoles
                        .Where(ur => ur.Role != null)
                        .Select(ur => ur.Role!.Name),
                    u.TelegramId,
                    u.IsTelegramVerified,
                    u.NotifyBeforeEvent
                })
                .ToListAsync();

            return Ok(users);
        }

        // POST api/User/{userId}/SetRole
        [HttpPost("{userId}/SetRole")]
        [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> SetUserRole(int userId, [FromBody] string roleName)
        {
            var user = await _db.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound("User not found");

            var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null)
                return BadRequest("Invalid role name");

            // Remove existing roles
            _db.UserRoles.RemoveRange(user.UserRoles);

            // Add new role
            user.UserRoles.Add(new UserRole
            {
                UserId = userId,
                RoleId = role.Id,
                User = user,
                Role = role
            });
            await _db.SaveChangesAsync();

            return Ok(new { Message = "Role updated successfully" });
        }

        // DELETE api/User/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Prevent deleting yourself
            var currentUserId = GetCurrentUserId();
            if (id == currentUserId)
            {
                return BadRequest(new { message = "Cannot delete your own account" });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize]
        [HttpPost("favorites/{eventId}")]
        public async Task<IActionResult> ToggleFavorite(int eventId)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                _logger.LogWarning("Attempt to toggle favorite without valid user ID");
                return Unauthorized(new { message = "Authentication required" });
            }

            try
            {
                var user = await _context.Users.FindAsync(userId.Value);
                var eventEntity = await _context.Events.FindAsync(eventId);

                if (user == null)
                {
                    _logger.LogWarning("User not found");
                    return NotFound(new { message = "User not found" });
                }

                if (eventEntity == null)
                {
                    _logger.LogWarning("Event not found");
                    return NotFound(new { message = "Event not found" });
                }

                var favorite = await _context.FavoriteEvents
                    .FirstOrDefaultAsync(f => f.EventId == eventId && f.UserId == userId.Value);

                if (favorite != null)
                {
                    _context.FavoriteEvents.Remove(favorite);
                }
                else
                {
                    favorite = new FavoriteEvent
                    {
                        UserId = userId.Value,
                        EventId = eventId,
                        User = user,
                        Event = eventEntity
                    };
                    _context.FavoriteEvents.Add(favorite);
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = favorite != null ? "Removed from favorites" : "Added to favorites" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling favorite for user {UserId} and event {EventId}", userId, eventId);
                return StatusCode(500, new { message = "An error occurred while updating favorites" });
            }
        }

        [Authorize]
        [HttpPost("planned/{eventId}")]
        public async Task<IActionResult> TogglePlanned(int eventId)
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                _logger.LogWarning("Attempt to toggle planned without valid user ID");
                return Unauthorized(new { message = "Authentication required" });
            }

            try
            {
                var user = await _context.Users.FindAsync(userId.Value);
                var eventEntity = await _context.Events.FindAsync(eventId);

                if (user == null)
                {
                    _logger.LogWarning("User not found");
                    return NotFound(new { message = "User not found" });
                }

                if (eventEntity == null)
                {
                    _logger.LogWarning("Event not found");
                    return NotFound(new { message = "Event not found" });
                }

                var planned = await _context.PlannedEvents
                    .FirstOrDefaultAsync(p => p.EventId == eventId && p.UserId == userId.Value);

                if (planned != null)
                {
                    _context.PlannedEvents.Remove(planned);
                }
                else
                {
                    planned = new PlannedEvent
                    {
                        UserId = userId.Value,
                        EventId = eventId,
                        User = user,
                        Event = eventEntity
                    };
                    _context.PlannedEvents.Add(planned);
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = planned != null ? "Removed from planned" : "Added to planned" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling planned for user {UserId} and event {EventId}", userId, eventId);
                return StatusCode(500, new { message = "An error occurred while updating planned events" });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    TelegramId = u.TelegramId,
                    IsTelegramVerified = u.IsTelegramVerified,
                    NotifyBeforeEvent = u.NotifyBeforeEvent,
                    Roles = u.UserRoles
                        .Where(ur => ur.Role != null)
                        .Select(ur => ur.Role!.Name)
                        .ToArray()
                })
                .ToListAsync();

            return users;
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "Authentication required" });
            }

            if (currentUserId != id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Where(u => u.Id == id)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    TelegramId = u.TelegramId,
                    IsTelegramVerified = u.IsTelegramVerified,
                    NotifyBeforeEvent = u.NotifyBeforeEvent,
                    Roles = u.UserRoles
                        .Where(ur => ur.Role != null)
                        .Select(ur => ur.Role!.Name)
                        .ToArray()
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return user;
        }

        [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        [HttpPost("{id}/toggle-ban")]
        public async Task<IActionResult> ToggleBan(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.IsBanned = !user.IsBanned;
            await _context.SaveChangesAsync();
            return Ok(new { user.Id, user.IsBanned });
        }

        [HttpPut("{id}")]
        [Authorize]
        [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> UpdateUser(int id, UserUpdateDto userDto)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return Unauthorized(new { message = "Authentication required" });
            }

            if (currentUserId != id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            user.TelegramId = userDto.TelegramId;
            user.IsTelegramVerified = userDto.IsTelegramVerified;
            user.NotifyBeforeEvent = userDto.NotifyBeforeEvent;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, $"Concurrency error updating user {id}");
                if (!UserExists(id))
                    return NotFound(new { message = "User not found" });
                throw;
            }

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
    
    
}
