using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventHub.Services
{
    public class NotificationHostedService : BackgroundService
    {
        private readonly ILogger<NotificationHostedService> _logger;

        public NotificationHostedService(ILogger<NotificationHostedService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("NotificationHostedService running at: {time}", DateTimeOffset.Now);
                
                // TODO: Implement notification logic here
                
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
} 