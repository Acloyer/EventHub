using System;
using System.Threading.Tasks;
using EventHub.Data;
using EventHub.Models;

namespace EventHub.Services
{
    public class NotificationService : INotificationService
    {
        private readonly EventHubDbContext _context;

        public NotificationService(EventHubDbContext context)
        {
            _context = context;
        }

        public async Task CreateNotificationAsync(int userId, string type, int entityId)
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = type,
                EntityId = entityId,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }
    }
} 