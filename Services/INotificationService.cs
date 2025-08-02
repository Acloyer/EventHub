using System.Threading.Tasks;

namespace EventHub.Services
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(int userId, string type, int entityId);
    }
} 