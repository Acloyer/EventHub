using System;
using System.Threading.Tasks;
using EventHub.Data;
using EventHub.Models;

namespace EventHub.Services
{
    public class NotificationService : INotificationService
    {
        private readonly EventHubDbContext _context;
        private readonly NotificationLocalizationService _localizationService;

        public NotificationService(EventHubDbContext context, NotificationLocalizationService localizationService)
        {
            _context = context;
            _localizationService = localizationService;
        }

        public async Task CreateNotificationAsync(int userId, string type, int entityId)
        {
            // Get user to determine preferred language
            var user = await _context.Users.FindAsync(userId);
            var userLanguage = user?.PreferredLanguage ?? "en";

            var notification = new Notification
            {
                UserId = userId,
                Type = type,
                EntityId = entityId,
                Message = GetNotificationMessage(type, entityId, userLanguage),
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        private string GetNotificationMessage(string type, int entityId, string language)
        {
            return type switch
            {
                "reaction" => _localizationService.GetNewReactionMessage(language, "your event"),
                "comment" => _localizationService.GetNewCommentMessage(language, "your event"),
                "EVENT_UPDATED" => _localizationService.GetEventUpdatedMessage(language, "your event"),
                "EVENT_CANCELLED" => _localizationService.GetEventCancelledMessage(language, "your event"),
                "REMINDER" => _localizationService.GetLocalizedMessage(language, "eventReminder", "your event", DateTime.Now.ToString("HH:mm")),
                _ => _localizationService.GetLocalizedMessage(language, "newNotification", "New notification")
            };
        }
    }
} 