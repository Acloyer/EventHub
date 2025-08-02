using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventHub.Data;
using EventHub.Models;
using System.Security.Claims;

namespace EventHub.Controllers
{
    [ApiController]
    [Route("api/activity-logs")]
    [Authorize(Roles = "SeniorAdmin,Owner")]
    public class ActivityLogController : ControllerBase
    {
        private readonly EventHubDbContext _context;
        private readonly ILogger<ActivityLogController> _logger;

        public ActivityLogController(EventHubDbContext context, ILogger<ActivityLogController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetActivityLogs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? action = null,
            [FromQuery] string? entityType = null,
            [FromQuery] int? userId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var query = _context.ActivityLogs
                    .Include(log => log.User)
                    .AsQueryable();

                // Фильтры
                if (!string.IsNullOrEmpty(action))
                {
                    query = query.Where(log => log.Action.Contains(action));
                }

                if (!string.IsNullOrEmpty(entityType))
                {
                    query = query.Where(log => log.EntityType == entityType);
                }

                if (userId.HasValue)
                {
                    query = query.Where(log => log.UserId == userId.Value);
                }

                if (startDate.HasValue)
                {
                    query = query.Where(log => log.Timestamp >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(log => log.Timestamp <= endDate.Value);
                }

                // Сортировка по времени (новые сначала)
                query = query.OrderByDescending(log => log.Timestamp);

                // Пагинация
                var totalCount = await query.CountAsync();
                var logs = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(log => new
                    {
                        log.Id,
                        log.UserId,
                        log.Action,
                        log.EntityType,
                        log.EntityId,
                        log.Details,
                        log.UserAgent,
                        log.Timestamp
                    })
                    .ToListAsync();

                return Ok(new
                {
                    Items = logs,
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activity logs");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetActivitySummary(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var query = _context.ActivityLogs.AsQueryable();

                if (startDate.HasValue)
                {
                    query = query.Where(log => log.Timestamp >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(log => log.Timestamp <= endDate.Value);
                }

                var summary = await query
                    .GroupBy(log => new { log.Action, log.EntityType })
                    .Select(g => new
                    {
                        Action = g.Key.Action,
                        EntityType = g.Key.EntityType,
                        Count = g.Count(),
                        LastOccurrence = g.Max(log => log.Timestamp)
                    })
                    .OrderByDescending(x => x.Count)
                    .ToListAsync();

                var userActivity = await query
                    .GroupBy(log => log.UserId)
                    .Select(g => new
                    {
                        UserId = g.Key,
                        ActionCount = g.Count(),
                        LastActivity = g.Max(log => log.Timestamp)
                    })
                    .OrderByDescending(x => x.ActionCount)
                    .Take(10)
                    .ToListAsync();

                return Ok(new
                {
                    Summary = summary,
                    TopUsers = userActivity
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving activity summary");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserActivityLogs(
            int userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var query = _context.ActivityLogs
                    .Include(log => log.User)
                    .Where(log => log.UserId == userId)
                    .OrderByDescending(log => log.Timestamp);

                var totalCount = await query.CountAsync();
                var logs = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(log => new
                    {
                        log.Id,
                        log.Action,
                        log.EntityType,
                        log.EntityId,
                        log.Details,
                        log.UserAgent,
                        log.Timestamp
                    })
                    .ToListAsync();

                return Ok(new
                {
                    Items = logs,
                    TotalCount = totalCount,
                    PageNumber = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user activity logs");
                return StatusCode(500, "Internal server error");
            }
        }
    }
} 