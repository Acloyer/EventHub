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

        public async Task LogActivityAsync(int userId, string action, string entityType, int? entityId, string details, string userAgent)
        {
            var log = new ActivityLog
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details,
                UserAgent = userAgent,
                Timestamp = DateTime.UtcNow
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }

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
        }
    }
} 