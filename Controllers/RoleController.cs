using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using EventHub.Data;
using EventHub.Models;
using EventHub.Models.DTOs;

namespace EventHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
    public class RoleController : ControllerBase
    {
        private readonly EventHubDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ILogger<RoleController> _logger;

        private static readonly Dictionary<string, int> RoleHierarchy = new()
        {
            ["User"] = 0,
            ["Organizer"] = 1,
            ["Admin"] = 2,
            ["SeniorAdmin"] = 3,
            ["Owner"] = 4
        };

        public RoleController(
            EventHubDbContext db,
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            ILogger<RoleController> logger)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var currentRole = await GetCurrentHighestRoleAsync();
            if (currentRole == null)
                return Forbid();

            var allRoles = await _roleManager.Roles.ToListAsync();
            var allowedRoles = allRoles.Where(r => RoleHierarchy.TryGetValue(r.Name, out var lvl)
                && lvl <= RoleHierarchy[currentRole]).ToList();

            var dto = new List<RoleDto>();
            foreach (var role in allowedRoles)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
                dto.Add(new RoleDto { Id = role.Id, Name = role.Name, UserCount = usersInRole.Count });
            }

            return Ok(dto);
        }

        private async Task<string?> GetCurrentHighestRoleAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);
            return roles.Where(r => RoleHierarchy.ContainsKey(r))
                        .OrderByDescending(r => RoleHierarchy[r])
                        .FirstOrDefault();
        }

        private async Task<bool> CanManageUserAsync(int targetUserId)
        {
            var currentRole = await GetCurrentHighestRoleAsync();
            if (currentRole == null) return false;

            var targetUser = await _userManager.FindByIdAsync(targetUserId.ToString());
            if (targetUser == null) return false;

            var targetRoles = await _userManager.GetRolesAsync(targetUser);
            var highestTarget = targetRoles.Where(r => RoleHierarchy.ContainsKey(r))
            .OrderByDescending(r => RoleHierarchy[r])
            .FirstOrDefault();

            return highestTarget == null || RoleHierarchy[currentRole] > RoleHierarchy[highestTarget];
        }

        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetUserRoles(int userId)
        {
            // if (!await CanManageUserAsync(userId)) return Forbid();

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return NotFound("User not found");

            var roles = await _userManager.GetRolesAsync(user);
            var currentId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (userId == currentId)
            {
                var list = roles.ToList();
                list.Insert(0, "You");
                return Ok(list);
            }

            return Ok(roles);
        }

        [HttpPost("user/{userId:int}/role/{roleName}")]
        public async Task<IActionResult> AddRoleToUser(int userId, string roleName)
        {
            if (!await CanManageUserAsync(userId))
                return Forbid();
            if (!await _roleManager.RoleExistsAsync(roleName))
                return NotFound("Role does not exist");

            // Only one Owner allowed
            if (roleName.ToLower() == "owner")
            {
                var existingOwners = await _userManager.GetUsersInRoleAsync("Owner");
                if (existingOwners.Count > 0)
                    return BadRequest(new { message = "There can be only one Owner." });
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return NotFound("User not found");

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = $"Role '{roleName}' added to user {userId}." });
        }

        [HttpDelete("user/{userId:int}/role/{roleName}")]
        public async Task<IActionResult> RemoveRoleFromUser(int userId, string roleName)
        {
            // if (!await CanManageUserAsync(userId)) return Forbid();

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return NotFound("User not found");

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok(new { message = $"Role '{roleName}' removed from user {userId}." });
        }
    }
}
