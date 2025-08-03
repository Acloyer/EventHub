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
using Telegram.Bot;
using Telegram.Bot.Types;
using User = EventHub.Models.User;
using WTelegram;

namespace EventHub.Controllers
{
    [Route("api/User")]
    [Authorize]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly EventHubDbContext _db;
        private readonly IUserService _userService;
        private readonly UserManager<User> _userManager;
        private readonly ITelegramBotClient _bot;
        private readonly ILogger<UserController> _logger;
        private readonly IConfiguration _config;
        private readonly IActivityLogService _activityLogService;
        private readonly JwtService _jwtService;
        
        public UserController(
            EventHubDbContext db,
            IUserService userService,
            UserManager<User> userManager,
            ITelegramBotClient bot,
            ILogger<UserController> logger,
            IConfiguration config,
            IActivityLogService activityLogService,
            JwtService jwtService)
        {
            _db = db;
            _userService = userService;
            _userManager = userManager;
            _bot = bot;
            _logger = logger;
            _config = config;
            _activityLogService = activityLogService;
            _jwtService = jwtService;
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }

        /// <summary>
        /// Extracts userId from JWT claim NameIdentifier
        /// </summary>
        private bool TryGetUserId(out int userId)
        {
            var raw = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(raw, out userId);
        }

        private static readonly Dictionary<string, int> RolePriority = new()
        {
            ["User"] = 0,
            ["Organizer"] = 1,
            ["Admin"] = 2,
            ["SeniorAdmin"] = 3,
            ["Owner"] = 4
        };

        private string GetUserAgent()
        {
            return HttpContext.Request.Headers.TryGetValue("User-Agent", out var userAgent) 
                ? userAgent.ToString() 
                : "Unknown";
        }

        /// <summary>
        /// GET: api/User/profile
        /// Returns profile by userId from token
        /// </summary>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return NotFound();

            // Проверяем и сбрасываем верификацию если TelegramId отсутствует
            await _userService.ValidateAndResetTelegramVerification(user);

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                IsBanned = user.IsBanned,
                Roles = roles.ToArray(),
                TelegramId = user.TelegramId,
                IsTelegramVerified = user.IsTelegramVerified,
                NotifyBeforeEvent = user.NotifyBeforeEvent
            });
        }

        // PATCH: api/Admin/Users/{userId}/telegram
        [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        [HttpPatch("{userId:int}/telegram")]
        public async Task<IActionResult> SetTelegramId(int userId, [FromBody] long telegramId)
        {
            var currentUser = GetCurrentUserId();
            if (!currentUser.HasValue) return Unauthorized();
            
            // Check if user is trying to modify themselves
            if (currentUser.Value == userId) 
                return BadRequest(new { message = "Cannot modify yourself through admin panel" });

            var currentUserEntity = await _userManager.FindByIdAsync(currentUser.Value.ToString());
            var targetUser = await _userManager.FindByIdAsync(userId.ToString());
            if (targetUser == null)
                return NotFound("User not found.");

            // Check role hierarchy
            var currentUserRoles = await _userManager.GetRolesAsync(currentUserEntity);
            var targetUserRoles = await _userManager.GetRolesAsync(targetUser);

            var currentUserMaxRole = currentUserRoles
                .Select(r => RolePriority.GetValueOrDefault(r, 0))
                .DefaultIfEmpty(0)
                .Max();

            var targetUserMaxRole = targetUserRoles
                .Select(r => RolePriority.GetValueOrDefault(r, 0))
                .DefaultIfEmpty(0)
                .Max();

            // Cannot modify user with higher rank
            if (targetUserMaxRole > currentUserMaxRole)
                return Forbid("Cannot modify user with higher role");

            targetUser.TelegramId = telegramId;
            await _userManager.UpdateAsync(targetUser);

            // Log Telegram ID change
            await _activityLogService.LogUserActivityAsync(
                currentUser.Value,
                "USER_TELEGRAM_UPDATED",
                $"Telegram ID {telegramId} set for user {targetUser.Email} by {currentUserEntity.Name}",
                GetUserAgent()
            );

            return Ok(new { message = $"Telegram ID {telegramId} set for user {targetUser.Email}" });
        }

        /// <summary>
        /// GET: api/User/created-events
        /// Returns events created by the current user
        /// </summary>
        [HttpGet("created-events")]
        public async Task<IActionResult> GetCreatedEvents([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            if (pageNumber <= 0 || pageSize <= 0)
                return BadRequest("Invalid pagination parameters.");

            var query = _db.Events
                .Include(e => e.Creator)
                .Where(e => e.CreatorId == userId);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(e =>
                    e.Title.Contains(searchTerm) ||
                    e.Description.Contains(searchTerm) ||
                    e.Category.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var events = await query
                .OrderByDescending(e => e.StartDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var eventDtos = events.Select(e => new EventDto(e)).ToList();

            return Ok(new
            {
                Items = eventDtos,
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }

        [Authorize]
        [HttpPost("set-notify")]
        public async Task<IActionResult> SetNotify([FromBody] bool notify)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return NotFound();

            user.NotifyBeforeEvent = notify;
            await _userManager.UpdateAsync(user);
            return Ok(new { notify });
        }

        // [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null)
        {
            if (pageNumber <= 0 || pageSize <= 0)
                return BadRequest("Invalid pagination parameters.");

            var query = _db.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(u =>
                    u.Name.Contains(searchTerm) ||
                    u.Email.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var users = await query
                .OrderBy(u => u.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Load roles separately
            var userDtos = new List<UserDto>();
            foreach (var user in users)
            {
                var dto = new UserDto(user);
                dto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
                userDtos.Add(dto);
            }

            return Ok(new
            {
                Items = userDtos,
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }
    
        [HttpGet("{id}")]
        // [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUser(int id)
        {   
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return NotFound();

            // Проверяем и сбрасываем верификацию если TelegramId отсутствует
            await _userService.ValidateAndResetTelegramVerification(user);

            var roles = await _userManager.GetRolesAsync(user);

            // Get current userId from token
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            bool isSelf = currentUserIdClaim != null && int.TryParse(currentUserIdClaim, out var currentUserId) && currentUserId == id;

            var userDto = new UserDto(user)
            {
                Roles = roles.ToList()
            };

            if (isSelf)
                userDto.Roles.Insert(0, "You");

            return Ok(userDto);
        }
        
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var currentUser = GetCurrentUserId();
            if (!currentUser.HasValue) return Unauthorized();
            
            // Check if user is trying to delete themselves
            if (currentUser.Value == id) 
                return BadRequest(new { message = "Cannot delete yourself" });

            // Check role hierarchy
            var currentUserEntity = await _userManager.FindByIdAsync(currentUser.Value.ToString());
            var targetUser = await _userManager.FindByIdAsync(id.ToString());
            
            if (targetUser == null) return NotFound();

            var currentUserRoles = await _userManager.GetRolesAsync(currentUserEntity);
            var targetUserRoles = await _userManager.GetRolesAsync(targetUser);

            var currentUserMaxRole = currentUserRoles
                .Select(r => RolePriority.GetValueOrDefault(r, 0))
                .DefaultIfEmpty(0)
                .Max();

            var targetUserMaxRole = targetUserRoles
                .Select(r => RolePriority.GetValueOrDefault(r, 0))
                .DefaultIfEmpty(0)
                .Max();

            // Cannot delete user with higher rank
            if (targetUserMaxRole > currentUserMaxRole)
                return Forbid("Cannot delete user with higher role");

            // Log user deletion
            await _activityLogService.LogUserActivityAsync(
                currentUser.Value,
                "USER_DELETED",
                $"User {targetUser.Name} ({targetUser.Email}) was deleted by {currentUserEntity.Name}",
                GetUserAgent()
            );

            await _userManager.DeleteAsync(targetUser);
            return NoContent();
        }

        [HttpPost("{id}/toggle-ban")]
        [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> ToggleBan(int id)
        {
            var currentUser = GetCurrentUserId();
            if (!currentUser.HasValue) return Unauthorized();
            
            // Check if user is trying to ban themselves
            if (currentUser.Value == id) 
                return BadRequest(new { message = "Cannot ban/unban yourself" });

            var currentUserEntity = await _userManager.FindByIdAsync(currentUser.Value.ToString());
            var targetUser = await _userManager.FindByIdAsync(id.ToString());
            if (targetUser == null) return NotFound();

            // Check role hierarchy
            var currentUserRoles = await _userManager.GetRolesAsync(currentUserEntity);
            var targetUserRoles = await _userManager.GetRolesAsync(targetUser);

            var currentUserMaxRole = currentUserRoles
                .Select(r => RolePriority.GetValueOrDefault(r, 0))
                .DefaultIfEmpty(0)
                .Max();

            var targetUserMaxRole = targetUserRoles
                .Select(r => RolePriority.GetValueOrDefault(r, 0))
                .DefaultIfEmpty(0)
                .Max();

            // Cannot ban user with higher rank
            if (targetUserMaxRole > currentUserMaxRole)
                return Forbid("Cannot ban/unban user with higher role");
            
            var wasBanned = targetUser.IsBanned;
            targetUser.IsBanned = !targetUser.IsBanned;
            await _userManager.UpdateAsync(targetUser);
            
            // Log ban status change
            await _activityLogService.LogUserActivityAsync(
                currentUser.Value,
                wasBanned ? "USER_UNBANNED" : "USER_BANNED",
                $"User {targetUser.Name} ({targetUser.Email}) was {(wasBanned ? "unbanned" : "banned")} by {currentUserEntity.Name}",
                GetUserAgent()
            );
            
            return Ok(new { targetUser.IsBanned });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto dto)
        {
            var currentUser = GetCurrentUserId();
            if (!currentUser.HasValue) return Unauthorized();
            
            // Check if user is trying to modify themselves
            if (currentUser.Value == id) 
                return BadRequest(new { message = "Cannot modify yourself through admin panel" });

            var currentUserEntity = await _userManager.FindByIdAsync(currentUser.Value.ToString());
            var targetUser = await _userManager.FindByIdAsync(id.ToString());
            if (targetUser == null) return NotFound();

            // Check role hierarchy
            var currentUserRoles = await _userManager.GetRolesAsync(currentUserEntity);
            var targetUserRoles = await _userManager.GetRolesAsync(targetUser);

            var currentUserMaxRole = currentUserRoles
                .Select(r => RolePriority.GetValueOrDefault(r, 0))
                .DefaultIfEmpty(0)
                .Max();

            var targetUserMaxRole = targetUserRoles
                .Select(r => RolePriority.GetValueOrDefault(r, 0))
                .DefaultIfEmpty(0)
                .Max();

            // Cannot modify user with higher rank
            if (targetUserMaxRole > currentUserMaxRole)
                return Forbid("Cannot modify user with higher role");

            // Update only allowed fields
            if (!string.IsNullOrWhiteSpace(dto.Name))
                targetUser.Name = dto.Name;
            
            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != targetUser.Email)
            {
                if (await _userManager.FindByEmailAsync(dto.Email) != null)
                    return BadRequest(new { message = "Email is already taken" });
                targetUser.Email = dto.Email;
                targetUser.UserName = dto.Email;
            }
            
            targetUser.IsBanned = dto.IsBanned;
            targetUser.TelegramId = dto.TelegramId;
            targetUser.IsTelegramVerified = dto.IsTelegramVerified;
            targetUser.NotifyBeforeEvent = dto.NotifyBeforeEvent;

            await _userManager.UpdateAsync(targetUser);
            
            // Log user modification
            await _activityLogService.LogUserActivityAsync(
                currentUser.Value,
                "USER_UPDATED",
                $"User {targetUser.Name} ({targetUser.Email}) was updated by {currentUserEntity.Name}",
                GetUserAgent()
            );
            
            return NoContent();
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileUpdateDto dto)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return NotFound();

            // Apply only safe fields for yourself:
            if (!string.IsNullOrWhiteSpace(dto.Name))
                user.Name = dto.Name;

            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {
                if (await _userManager.FindByEmailAsync(dto.Email) != null)
                    return BadRequest("Email is already taken");
                user.Email    = dto.Email;
                user.UserName = dto.Email;
            }

            user.TelegramId        = dto.TelegramId;
            user.NotifyBeforeEvent = dto.NotifyBeforeEvent;
            // user.IsBanned - don't touch!

            var res = await _userManager.UpdateAsync(user);
            if (!res.Succeeded) return BadRequest(res.Errors);

            return Ok(new UserDto(user));
        }

        [Authorize]
        [HttpGet("link-telegram")]
        public IActionResult GetTelegramLink()
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var botUsername = "eventthub_bot"; 
            var link = $"https://t.me/{botUsername}?start={userId.Value}";
            return Ok(new { LinkUrl = link });
        }

                
        [Authorize]
        [HttpPost("confirm-telegram")]
        public async Task<IActionResult> ConfirmTelegramCode([FromBody] ConfirmTelegramCodeDto dto)
        {
            // 1) Get user from token
            var user = await _userManager.GetUserAsync(User);
            if (user == null) 
                return Unauthorized();

            // 2) Check that code matches
            if (user.TelegramCode != dto.Code)
                return BadRequest(new { message = "Invalid code." });

            // 3) Complete verification
            user.IsTelegramVerified = true;
            user.TelegramCode       = null;
            await _userManager.UpdateAsync(user);

            // 4) Send congratulations to Telegram
            if (user.TelegramId.HasValue)
            {
                await _bot.SendTextMessageAsync(
                    chatId: user.TelegramId.Value,
                    text: "Congratulations! You have been successfully verified in our service."
                );
            }

            return Ok(new { message = "Telegram confirmed." });
        }

        [Authorize]
        [HttpPost("{userId:int}/start-delete-confirmation")]
        public async Task<IActionResult> StartDeleteConfirmation(int userId)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue || currentUserId.Value != userId)
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return NotFound();

            if (!user.IsTelegramVerified)
                return BadRequest(new { message = "User must be Telegram verified to perform this action." });

            // Generate confirmation code
            var random = new Random();
            var confirmationCode = random.Next(100000, 999999);
            user.TelegramCode = confirmationCode;
            await _userManager.UpdateAsync(user);

            // Send confirmation code to Telegram
            if (user.TelegramId.HasValue)
            {
                await _bot.SendTextMessageAsync(
                    chatId: user.TelegramId.Value,
                    text: $"⚠️ Database deletion confirmation code:\n\n{confirmationCode}\n\nThis code is required to confirm the deletion of all database data."
                );
            }

            return Ok(new { message = "Confirmation code sent to Telegram." });
        }

        [Authorize]
        [HttpPost("start-seed-confirmation")]
        public async Task<IActionResult> StartSeedConfirmation()
        {
            // 1) Get user from token
            var user = await _userManager.GetUserAsync(User);
            if (user == null) 
                return Unauthorized();

            // 2) Check if user has Telegram connected
            if (!user.TelegramId.HasValue)
                return BadRequest(new { message = "Telegram not connected. Please connect Telegram in your profile first." });

            // 3) Generate confirmation code
            var random = new Random();
            var confirmationCode = random.Next(100000, 999999);
            user.TelegramCode = confirmationCode;
            await _userManager.UpdateAsync(user);

            // Send confirmation code to Telegram
            if (user.TelegramId.HasValue)
            {
                await _bot.SendTextMessageAsync(
                    chatId: user.TelegramId.Value,
                    text: $"🔐 Database access confirmation code:\n\n{confirmationCode}\n\nThis code is required to access database management operations."
                );
            }

            return Ok(new { message = "Confirmation code sent to Telegram." });
        }

        [Authorize]
        [HttpPost("start-ownership-confirmation")]
        public async Task<IActionResult> StartOwnershipConfirmation(int userId)
        {
            // 1) Get user from token
            var user = await _userManager.GetUserAsync(User);
            if (user == null) 
                return Unauthorized();

            // 2) Check if user has Telegram connected
            if (!user.TelegramId.HasValue)
                return BadRequest(new { message = "Telegram not connected. Please connect Telegram in your profile first." });

            // 3) Generate confirmation code
            var random = new Random();
            var confirmationCode = random.Next(100000, 999999);
            user.TelegramCode = confirmationCode;
            await _userManager.UpdateAsync(user);

            // Send confirmation code to Telegram
            if (user.TelegramId.HasValue)
            {
                await _bot.SendTextMessageAsync(
                    chatId: user.TelegramId.Value,
                    text: $"🔐⚠️ Ownership confirmation code:\n\n{confirmationCode}\n\nYou are entering the owner transfer section. Please verify your identity via Telegram to continue this critical operation."
                );
            }

            return Ok(new { message = "Confirmation code sent to Telegram." });
        }

        [Authorize]
        [HttpPost("confirm-delete")]
        public async Task<IActionResult> ConfirmDeleteCode([FromBody] ConfirmTelegramCodeDto dto)
        {
            // 1) Get user from token
            var user = await _userManager.GetUserAsync(User);
            if (user == null) 
                return Unauthorized();

            // 2) Check that code matches
            if (user.TelegramCode != dto.Code)
                return BadRequest(new { message = "Invalid confirmation code." });

            // 3) Clear the code
            user.TelegramCode = null;
            await _userManager.UpdateAsync(user);

            return Ok(new { message = "Delete confirmation successful." });
        }

        /// <summary>
        /// POST: api/User/impersonate/{targetUserId}
        /// Allows Owner to impersonate another user by generating a temporary token
        /// </summary>
        [Authorize(Roles = "Owner")]
        [HttpPost("impersonate/{targetUserId:int}")]
        public async Task<IActionResult> ImpersonateUser(int targetUserId)
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue) return Unauthorized();

            // Get current user (Owner)
            var currentUser = await _userManager.FindByIdAsync(currentUserId.Value.ToString());
            if (currentUser == null) return NotFound("Current user not found");

            // Get target user
            var targetUser = await _userManager.FindByIdAsync(targetUserId.ToString());
            if (targetUser == null) return NotFound("Target user not found");

            // Prevent self-impersonation
            if (currentUserId.Value == targetUserId)
                return BadRequest(new { message = "You cannot impersonate yourself" });

            // Get target user roles
            var targetUserRoles = await _userManager.GetRolesAsync(targetUser);

            // Generate impersonation token
            var impersonationToken = _jwtService.GenerateImpersonationToken(targetUser, targetUserRoles, currentUserId.Value);

            // Log the impersonation action
            await _activityLogService.LogUserActivityAsync(
                currentUserId.Value,
                "USER_IMPERSONATION",
                $"Owner {currentUser.Name} ({currentUser.Email}) impersonated user {targetUser.Name} ({targetUser.Email})",
                GetUserAgent()
            );

            return Ok(new
            {
                message = $"Successfully generated impersonation token for {targetUser.Name}",
                impersonationToken = impersonationToken,
                targetUser = new UserDto(targetUser)
                {
                    Roles = targetUserRoles.ToList()
                }
            });
        }

        /// <summary>
        /// GET: api/User/roles
        /// Returns all available roles
        /// </summary>
        [HttpGet("roles")]
        [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _db.Roles.ToListAsync();
            return Ok(roles.Select(r => new { r.Id, r.Name }));
        }

        /// <summary>
        /// POST: api/User/{userId}/roles
        /// Assigns roles to a user
        /// </summary>
        [HttpPost("{userId:int}/roles")]
        [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> AssignRoles(int userId, [FromBody] AssignRolesDto dto)
        {
            var currentUser = GetCurrentUserId();
            if (!currentUser.HasValue) return Unauthorized();
            
            // Check if user is trying to modify themselves
            if (currentUser.Value == userId) 
                return BadRequest(new { message = "Cannot modify your own roles through admin panel" });

            var currentUserEntity = await _userManager.FindByIdAsync(currentUser.Value.ToString());
            var targetUser = await _userManager.FindByIdAsync(userId.ToString());
            if (targetUser == null) return NotFound();

            // Check role hierarchy
            var currentUserRoles = await _userManager.GetRolesAsync(currentUserEntity);
            var targetUserRoles = await _userManager.GetRolesAsync(targetUser);

            var currentUserMaxRole = currentUserRoles
                .Select(r => RolePriority.GetValueOrDefault(r, 0))
                .DefaultIfEmpty(0)
                .Max();

            var targetUserMaxRole = targetUserRoles
                .Select(r => RolePriority.GetValueOrDefault(r, 0))
                .DefaultIfEmpty(0)
                .Max();

            // Cannot modify roles of user with higher rank
            if (targetUserMaxRole > currentUserMaxRole)
                return Forbid("Cannot modify roles of user with higher role");

            // Check that user is not trying to assign roles higher than their own
            var newRolesMaxPriority = dto.Roles
                .Select(r => RolePriority.GetValueOrDefault(r, 0))
                .DefaultIfEmpty(0)
                .Max();

            if (newRolesMaxPriority > currentUserMaxRole)
                return Forbid("Cannot assign roles higher than your own");

            // Remove all current roles
            var currentRoles = await _userManager.GetRolesAsync(targetUser);
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(targetUser, currentRoles);
            }

            // Add new roles
            if (dto.Roles.Any())
            {
                await _userManager.AddToRolesAsync(targetUser, dto.Roles);
            }

            // Log role changes
            await _activityLogService.LogUserActivityAsync(
                currentUser.Value,
                "USER_ROLES_UPDATED",
                $"Roles for user {targetUser.Name} ({targetUser.Email}) were updated to [{string.Join(", ", dto.Roles)}] by {currentUserEntity.Name}",
                GetUserAgent()
            );

            return Ok(new { message = "Roles updated successfully" });
        }

        /// <summary>
        /// GET: api/User/dashboard-stats
        /// Returns dashboard statistics for admin users
        /// </summary>
        [HttpGet("dashboard-stats")]
        [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var stats = new
                {
                    users = await _db.Users.CountAsync(),
                    events = await _db.Events.CountAsync(),
                    comments = await _db.EventComments.CountAsync(),
                    logs = await _db.ActivityLogs.CountAsync()
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting dashboard stats");
                return StatusCode(500, new { message = "Error occurred while getting dashboard stats", error = ex.Message });
            }
        }
    }
}