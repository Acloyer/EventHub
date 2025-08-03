using EventHub.Data;
using EventHub.Models;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Services
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly EventHubDbContext _context;

        public ActivityLogService(EventHubDbContext context)
        {
            _context = context;
        }

<<<<<<< HEAD
        public async Task LogActivityAsync(int userId, string action, string entityType, int? entityId, string details, string userAgent)
=======
<<<<<<< HEAD
        public async Task LogActivityAsync(int userId, string action, string entityType, int? entityId, string details, string userAgent)
=======
        public async Task LogActivityAsync(int userId, string action, string entityType, int? entityId, string details, string ipAddress, string userAgent)
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
        {
            var log = new ActivityLog
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details,
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
                IpAddress = ipAddress,
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
                UserAgent = userAgent,
                Timestamp = DateTime.UtcNow
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }

<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
        public async Task LogUserActivityAsync(int userId, string action, string details, string userAgent)
        {
            await LogActivityAsync(userId, action, "User", userId, details, userAgent);
        }

        public async Task LogEventActivityAsync(int userId, string action, int eventId, string details, string userAgent)
        {
            await LogActivityAsync(userId, action, "Event", eventId, details, userAgent);
        }

        public async Task LogCommentActivityAsync(int userId, string action, int commentId, string details, string userAgent)
        {
            await LogActivityAsync(userId, action, "Comment", commentId, details, userAgent);
<<<<<<< HEAD
=======
=======
        public async Task LogUserActivityAsync(int userId, string action, string details, string ipAddress, string userAgent)
        {
            await LogActivityAsync(userId, action, "User", userId, details, ipAddress, userAgent);
        }

        public async Task LogEventActivityAsync(int userId, string action, int eventId, string details, string ipAddress, string userAgent)
        {
            await LogActivityAsync(userId, action, "Event", eventId, details, ipAddress, userAgent);
        }

        public async Task LogCommentActivityAsync(int userId, string action, int commentId, string details, string ipAddress, string userAgent)
        {
            await LogActivityAsync(userId, action, "Comment", commentId, details, ipAddress, userAgent);
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
        }
    }
} 