using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventHub.Data;
using EventHub.Models;

namespace EventHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly EventHubDbContext _db;
        public RoleController(EventHubDbContext db) => _db = db;

        // ── GET api/role ────────────────────────────────────────────────
        // Только Admin и Moderator могут получить весь список ролей
        [HttpGet]
        [Authorize(Roles = "Admin,Moderator")]
        public async Task<IActionResult> GetAllRoles()
            => Ok(await _db.Roles.Select(r => r.Name).ToListAsync());

        // ── GET api/role/user/{userId} ──────────────────────────────────
        // Admin и Organizer могут смотреть, какие роли у конкретного юзера
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> GetUserRoles(int userId)
        {
            var roles = await _db.UserRoles
                .Where(ur => ur.UserId == userId)
                .Include(ur => ur.Role)
                .Select(ur => ur.Role.Name)
                .ToListAsync();
            return Ok(roles);
        }

        public class RoleAssignDto
        {
            public int UserId { get; set; }
            public string Role { get; set; } = null!;
        }

        // ── POST api/role/assign ────────────────────────────────────────
        // Только Admin может выдать роль
        [HttpPost("assign")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRole([FromBody] RoleAssignDto dto)
        {
            var role = await _db.Roles.SingleOrDefaultAsync(r => r.Name == dto.Role);
            if (role == null) return NotFound("Role not found");

            if (await _db.UserRoles.AnyAsync(ur => ur.UserId == dto.UserId && ur.RoleId == role.Id))
                return BadRequest("User already has this role");

            _db.UserRoles.Add(new UserRole { UserId = dto.UserId, RoleId = role.Id });
            await _db.SaveChangesAsync();
            return Ok();
        }

        // ── POST api/role/remove ───────────────────────────────────────
        // Только Admin может снять роль
        [HttpPost("remove")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveRole([FromBody] RoleAssignDto dto)
        {
            var ur = await _db.UserRoles
                .Include(x => x.Role)
                .SingleOrDefaultAsync(ur => ur.UserId == dto.UserId && ur.Role.Name == dto.Role);

            if (ur == null) return NotFound("Role assignment not found");

            _db.UserRoles.Remove(ur);
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}
