using System.Threading.Tasks;
using EventHub.Data;
using EventHub.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
<<<<<<< HEAD
using Microsoft.EntityFrameworkCore;
=======
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c

namespace EventHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly ILogger<SeedController> _logger;
<<<<<<< HEAD
        private readonly EventHubDbContext _db;

        public SeedController(ILogger<SeedController> logger, EventHubDbContext db)
        {
            _logger = logger;
            _db = db;
=======

        public SeedController(ILogger<SeedController> logger)
        {
            _logger = logger;
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
        }

        [HttpPost("seed")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> SeedDatabase([FromBody] SeedDataDto? seedDataDto = null)
        {
            try
            {
                _logger.LogInformation("Starting database seeding...");
                
                if (seedDataDto != null)
                {
                    _logger.LogInformation($"Seed data received: Owner={seedDataDto.OwnerCount}, SeniorAdmin={seedDataDto.SeniorAdminCount}, Admin={seedDataDto.AdminCount}, Organizer={seedDataDto.OrganizerCount}, RegularUser={seedDataDto.RegularUserCount}");
                }
                else
                {
                    _logger.LogInformation("No seed data provided, using defaults");
                }
                
                await SeedData.Initialize(HttpContext.RequestServices, seedDataDto);
                
                _logger.LogInformation("Database seeding completed successfully!");
                
                return Ok(new { message = "Database seeded successfully with test data!" });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred during database seeding");
                return StatusCode(500, new { message = "Error occurred during database seeding", error = ex.Message });
            }
        }
<<<<<<< HEAD

        [HttpGet("stats")]
        [Authorize(Roles = "Owner")]
        public async Task<IActionResult> GetDatabaseStats()
        {
            try
            {
                var stats = new
                {
                    users = await _db.Users.CountAsync(),
                    usersLastCreated = await _db.Users.OrderByDescending(u => u.Id).Select(u => (DateTime?)DateTime.UtcNow).FirstOrDefaultAsync(), // Using current time as placeholder
                    events = await _db.Events.CountAsync(),
                    eventsLastCreated = await _db.Events.OrderByDescending(e => e.StartDate).Select(e => (DateTime?)e.StartDate).FirstOrDefaultAsync(),
                    comments = await _db.EventComments.CountAsync(),
                    commentsLastCreated = await _db.EventComments.OrderByDescending(c => c.PostDate).Select(c => (DateTime?)c.PostDate).FirstOrDefaultAsync(),
                    reactions = await _db.PostReactions.CountAsync(),
                    reactionsLastCreated = await _db.PostReactions.OrderByDescending(r => r.Id).Select(r => (DateTime?)DateTime.UtcNow).FirstOrDefaultAsync(), // Using current time as placeholder
                    favorites = await _db.FavoriteEvents.CountAsync(),
                    favoritesLastCreated = await _db.FavoriteEvents.OrderByDescending(f => f.EventId).Select(f => (DateTime?)DateTime.UtcNow).FirstOrDefaultAsync(), // Using current time as placeholder
                    plannedEvents = await _db.PlannedEvents.CountAsync(),
                    plannedEventsLastCreated = await _db.PlannedEvents.OrderByDescending(p => p.CreatedAt).Select(p => (DateTime?)p.CreatedAt).FirstOrDefaultAsync()
                };

                return Ok(stats);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting database stats");
                return StatusCode(500, new { message = "Error occurred while getting database stats", error = ex.Message });
            }
        }
=======
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
    }
} 