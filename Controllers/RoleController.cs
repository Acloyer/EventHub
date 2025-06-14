using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventHub.Data;
using EventHub.Models;
using Microsoft.Extensions.Logging;

namespace EventHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly EventHubDbContext _db;
        private readonly ILogger<RoleController> _logger;

        public RoleController(EventHubDbContext db, ILogger<RoleController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // ── GET api/Role/names ────────────────────────────────────────────────
        // Returns only the list of role names. Admin and Organizer may call this.
        [HttpGet("names")]
        [Authorize(Roles = "Admin,Organizer,SeniorAdmin,Owner")]
        public async Task<IActionResult> GetAllRoleNames()
        {
            var roleNames = await _db.Roles
                                     .Select(r => r.Name)
                                     .ToListAsync();
            return Ok(roleNames);
        }

        // ── GET api/Role ──────────────────────────────────────────────────────
        // Returns the full Role objects (with UserRoles included). Admin only.
        [HttpGet]
        [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
        {
            var roles = await _db.Roles
                                 .Include(r => r.UserRoles)
                                 .ToListAsync();

            // We project into a fresh Role instance so that EF’s tracker warnings (null issues) are avoided.
            var result = roles.Select(r => new Role
            {
                Id = r.Id,
                Name = r.Name,
                UserRoles = r.UserRoles ?? new List<UserRole>()
            }).ToList();

            return Ok(result);
        }

        // ── GET api/Role/user/{userId} ────────────────────────────────────────
        // Returns the names of roles assigned to a specific user. Admin and Organizer may call this.
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin,Organizer,SeniorAdmin,Owner")]
        public async Task<IActionResult> GetUserRoles(int userId)
        {
            var roles = await _db.UserRoles
                                 .Include(ur => ur.Role)
                                 .Where(ur => ur.UserId == userId && ur.Role != null)
                                 .Select(ur => ur.Role!.Name)
                                 .ToListAsync();

            return Ok(roles);
        }

        public class RoleAssignDto
        {
            public int UserId { get; set; }
            public string Role { get; set; } = null!;
        }

        // ── POST api/Role/assign ──────────────────────────────────────────────
        // Assigns a role (by roleId) to a user. Admin only.
        [HttpPost("assign")]
        [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> AssignRole(int userId, int roleId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found");

            var role = await _db.Roles.FindAsync(roleId);
            if (role == null)
                return NotFound("Role not found");

            // Check if that exact UserRole already exists
            bool alreadyHas = await _db.UserRoles
                                      .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
            if (alreadyHas)
                return BadRequest("User already has that role");

            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = roleId
            };

            _db.UserRoles.Add(userRole);
            await _db.SaveChangesAsync();
            return Ok();
        }

        // ── POST api/Role/remove ──────────────────────────────────────────────
        // Removes the named role from a user. Admin only.
        [HttpPost("remove")]
        [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> RemoveRole([FromBody] RoleAssignDto dto)
        {
            var ur = await _db.UserRoles
                              .Include(x => x.Role)
                              .SingleOrDefaultAsync(ur => ur.UserId == dto.UserId 
                                                         && ur.Role != null 
                                                         && ur.Role.Name == dto.Role);

            if (ur == null)
                return NotFound("Role assignment not found");

            _db.UserRoles.Remove(ur);
            await _db.SaveChangesAsync();
            return Ok();
        }

        // ── GET api/Role/{id} ─────────────────────────────────────────────────
        // Returns a single Role (with its UserRoles). Admin only.
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<ActionResult<Role>> GetRole(int id)
        {
            var role = await _db.Roles
                                .Include(r => r.UserRoles)
                                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
                return NotFound(new { message = "Role not found" });

            // Return a fresh object so that “UserRoles” is never null
            return Ok(new Role
            {
                Id = role.Id,
                Name = role.Name,
                UserRoles = role.UserRoles ?? new List<UserRole>()
            });
        }

        // ── POST api/Role ─────────────────────────────────────────────────────
        // Creates a new role. Admin only.
        [HttpPost]
        [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<ActionResult<Role>> CreateRole(Role role)
        {
            if (await _db.Roles.AnyAsync(r => r.Name == role.Name))
            {
                return BadRequest(new { message = "Role already exists" });
            }

            // Initialize UserRoles since it’s required
            role.UserRoles = role.UserRoles ?? new List<UserRole>();
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRole), new { id = role.Id }, role);
        }

        // ── DELETE api/Role/{id} ──────────────────────────────────────────────
        // Deletes a role. Admin only.
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,SeniorAdmin,Owner")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _db.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound(new { message = "Role not found" });
            }

            _db.Roles.Remove(role);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
