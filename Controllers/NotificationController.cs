using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using EventHub.Data;
using EventHub.Models;

namespace EventHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly EventHubDbContext _context;
        private readonly UserManager<User> _userManager;

        public NotificationController(EventHubDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetNotifications()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50) // Limit to last 50 notifications
                .ToListAsync();

            // Get all unique entity IDs for batch lookup
            var eventIds = notifications.Where(n => n.Type.StartsWith("event")).Select(n => n.EntityId).Distinct().ToList();
            var userIds = notifications.Where(n => n.Type == "comment" || n.Type == "reaction").Select(n => n.EntityId).Distinct().ToList();

            // Batch fetch events and users
            var events = await _context.Events.Where(e => eventIds.Contains(e.Id)).ToDictionaryAsync(e => e.Id, e => e.Title);
            var users = await _context.Users.Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.UserName);

            // Process the results
            var result = notifications.Select(n => new
            {
                n.Id,
                n.UserId,
                n.Type,
                n.EntityId,
                n.CreatedAt,
                n.IsRead,
                Message = GetNotificationMessage(n.Type, n.EntityId),
                EventTitle = n.Type.StartsWith("event") ? events.GetValueOrDefault(n.EntityId) : null,
                UserName = (n.Type == "comment" || n.Type == "reaction") ? users.GetValueOrDefault(n.EntityId) : null
            });

            return Ok(result);
        }

        [HttpPost("{id}/mark-read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notification == null)
            {
                return NotFound("Notification not found");
            }

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notification == null)
            {
                return NotFound("Notification not found");
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();

            return Ok();
        }

        private static string GetNotificationMessage(string type, int entityId)
        {
            return type switch
            {
                "comment" => "Someone commented on an event",
                "reaction" => "Someone reacted to your comment",
                "event" => "A new event was created",
                "event_deleted" => "An event was deleted",
                "event_reminder" => "Event reminder",
                "event_starting" => "Event is starting soon",
                _ => "New notification"
            };
        }


    }
} 