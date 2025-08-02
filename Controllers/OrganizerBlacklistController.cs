using System.Security.Claims;
using EventHub.Data;
using EventHub.Models;
using EventHub.Models.DTOs;
using EventHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace EventHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrganizerBlacklistController : ControllerBase
    {
        private readonly EventHubDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly IActivityLogService _activityLogService;

        public OrganizerBlacklistController(
            EventHubDbContext db,
            UserManager<User> userManager,
            IActivityLogService activityLogService)
        {
            _db = db;
            _userManager = userManager;
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
            
            return roles.Contains("Owner") || 
                   roles.Contains("SeniorAdmin") || 
                   roles.Contains("Admin") || 
                   roles.Contains("Organizer");
        }

        /// <summary>
        /// GET: api/OrganizerBlacklist/my-blacklist
        /// Get the current organizer's blacklist
        /// </summary>
        [HttpGet("my-blacklist")]
        public async Task<IActionResult> GetMyBlacklist()
        {
            var organizerId = GetCurrentUserId();
            if (!organizerId.HasValue)
                return Unauthorized();

            if (!await IsUserOrganizer(organizerId.Value))
                return Forbid();

            var blacklist = await _db.OrganizerBlacklists
                .Include(ob => ob.BannedUser)
                .Where(ob => ob.OrganizerId == organizerId.Value)
                .OrderByDescending(ob => ob.CreatedAt)
                .Select(ob => new OrganizerBlacklistDto
                {
                    Id = ob.Id,
                    OrganizerId = ob.OrganizerId,
                    BannedUserId = ob.BannedUserId,
                    CreatedAt = ob.CreatedAt,
                    Reason = ob.Reason,
                    BannedUserName = ob.BannedUser.Name,
                    BannedUserEmail = ob.BannedUser.Email
                })
                .ToListAsync();

            return Ok(blacklist);
        }

        /// <summary>
        /// POST: api/OrganizerBlacklist/add
        /// Add user to blacklist
        /// </summary>
        [HttpPost("add")]
        public async Task<IActionResult> AddToBlacklist([FromBody] CreateBlacklistEntryDto dto)
        {
            var organizerId = GetCurrentUserId();
            if (!organizerId.HasValue)
                return Unauthorized();

            if (!await IsUserOrganizer(organizerId.Value))
                return Forbid();

            // Check that user is not trying to ban themselves
            if (organizerId.Value == dto.BannedUserId)
                return BadRequest("You cannot ban yourself");

            // Check that user exists
            var bannedUser = await _userManager.FindByIdAsync(dto.BannedUserId.ToString());
            if (bannedUser == null)
                return NotFound("User not found");

            // Check that user is not already in blacklist
            var existingEntry = await _db.OrganizerBlacklists
                .FirstOrDefaultAsync(ob => ob.OrganizerId == organizerId.Value && ob.BannedUserId == dto.BannedUserId);

            if (existingEntry != null)
                return BadRequest("User is already in your blacklist");

            var blacklistEntry = new OrganizerBlacklist
            {
                OrganizerId = organizerId.Value,
                BannedUserId = dto.BannedUserId,
                Reason = dto.Reason,
                CreatedAt = DateTime.UtcNow
            };

            _db.OrganizerBlacklists.Add(blacklistEntry);
            await _db.SaveChangesAsync();

            // Log the action
            await _activityLogService.LogUserActivityAsync(
                organizerId.Value,
                "USER_ADDED_TO_BLACKLIST",
                $"Added user {bannedUser.Name} ({bannedUser.Email}) to blacklist. Reason: {dto.Reason ?? "No reason provided"}",
<<<<<<< HEAD
=======
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
                HttpContext.Request.Headers["User-Agent"].ToString()
            );

            return Ok(new { message = "User added to blacklist successfully" });
        }

        /// <summary>
        /// DELETE: api/OrganizerBlacklist/remove
        /// Remove user from blacklist
        /// </summary>
        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveFromBlacklist([FromBody] RemoveBlacklistEntryDto dto)
        {
            var organizerId = GetCurrentUserId();
            if (!organizerId.HasValue)
                return Unauthorized();

            if (!await IsUserOrganizer(organizerId.Value))
                return Forbid();

            var blacklistEntry = await _db.OrganizerBlacklists
                .Include(ob => ob.BannedUser)
                .FirstOrDefaultAsync(ob => ob.OrganizerId == organizerId.Value && ob.BannedUserId == dto.BannedUserId);

            if (blacklistEntry == null)
                return NotFound("User not found in blacklist");

            _db.OrganizerBlacklists.Remove(blacklistEntry);
            await _db.SaveChangesAsync();

            // Логируем действие
            await _activityLogService.LogUserActivityAsync(
                organizerId.Value,
                "USER_REMOVED_FROM_BLACKLIST",
                $"Removed user {blacklistEntry.BannedUser.Name} ({blacklistEntry.BannedUser.Email}) from blacklist",
<<<<<<< HEAD
=======
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
                HttpContext.Request.Headers["User-Agent"].ToString()
            );

            return Ok(new { message = "User removed from blacklist successfully" });
        }

        /// <summary>
        /// GET: api/OrganizerBlacklist/check/{userId}
        /// Проверить, находится ли пользователь в черном списке текущего организатора
        /// </summary>
        [HttpGet("check/{userId:int}")]
        public async Task<IActionResult> CheckBlacklistStatus(int userId)
        {
            var organizerId = GetCurrentUserId();
            if (!organizerId.HasValue)
                return Unauthorized();

            var isBlacklisted = await _db.OrganizerBlacklists
                .AnyAsync(ob => ob.OrganizerId == organizerId.Value && ob.BannedUserId == userId);

            return Ok(new { isBlacklisted });
        }
    }
} 