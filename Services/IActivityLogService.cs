using EventHub.Models;

namespace EventHub.Services
{
    public interface IActivityLogService
    {
        Task LogActivityAsync(int userId, string action, string entityType, int? entityId, string details, string ipAddress, string userAgent);
        Task LogUserActivityAsync(int userId, string action, string details, string ipAddress, string userAgent);
        Task LogEventActivityAsync(int userId, string action, int eventId, string details, string ipAddress, string userAgent);
        Task LogCommentActivityAsync(int userId, string action, int commentId, string details, string ipAddress, string userAgent);
    }
} 