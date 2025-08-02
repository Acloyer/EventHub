using System.Threading.Tasks;
using EventHub.Data;
using EventHub.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EventHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly ILogger<SeedController> _logger;

        public SeedController(ILogger<SeedController> logger)
        {
            _logger = logger;
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
    }
} 