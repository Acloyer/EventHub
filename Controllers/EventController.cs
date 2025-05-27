using EventHub.Data;
using EventHub.Models;
using EventHub.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EventHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly EventHubDbContext _db;
        public EventController(EventHubDbContext db) => _db = db;

        // GET: api/Event
        [HttpGet]
        [AllowAnonymous] // любой может смотреть список
        public async Task<IActionResult> GetAll()
        {
            var events = await _db.Events
                                  .Include(e => e.Organizer)
                                  .Select(e => new {
                                      e.Id,
                                      e.Title,
                                      e.Description,
                                      e.StartTime,
                                      e.EndTime,
                                      OrganizerEmail = e.Organizer.Email
                                  })
                                  .ToListAsync();
            return Ok(events);
        }

        // GET: api/Event/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(int id)
        {
            var ev = await _db.Events
                              .Include(e => e.Organizer)
                              .FirstOrDefaultAsync(e => e.Id == id);
            if (ev == null) return NotFound();
            return Ok(new
            {
                ev.Id,
                ev.Title,
                ev.Description,
                ev.StartTime,
                ev.EndTime,
                OrganizerEmail = ev.Organizer.Email
            });
        }

        // POST: api/Event
        [HttpPost]
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> Create([FromBody] EventDto dto)
        {
            // получим текущего пользователя из токена
            var subClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
               ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (subClaim == null)
                return Unauthorized();   // или BadRequest()
            var userId = int.Parse(subClaim);

            var ev = new Event
            {
                Title = dto.Title,
                Description = dto.Description,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                OrganizerId = userId
            };
            _db.Events.Add(ev);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = ev.Id }, ev);
        }

        // PUT: api/Event/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> Update(int id, [FromBody] EventDto dto)
        {
            var ev = await _db.Events.FindAsync(id);
            if (ev == null) return NotFound();

            // если не админ — проверяем что организатор совпадает
            if (!User.IsInRole("Admin") && ev.OrganizerId != int.Parse(User.FindFirst("sub")!.Value))
                return Forbid();

            ev.Title = dto.Title;
            ev.Description = dto.Description;
            ev.StartTime = dto.StartTime;
            ev.EndTime = dto.EndTime;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Event/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> Delete(int id)
        {
            var ev = await _db.Events.FindAsync(id);
            if (ev == null) return NotFound();

            if (!User.IsInRole("Admin") && ev.OrganizerId != int.Parse(User.FindFirst("sub")!.Value))
                return Forbid();

            _db.Events.Remove(ev);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
