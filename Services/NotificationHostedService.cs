using EventHub.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EventHub.Services
{
    public class NotificationHostedService : BackgroundService
    {
        private readonly ILogger<NotificationHostedService> _logger;
        private readonly IServiceProvider _services;
        private readonly IHttpClientFactory _httpFactory;
        private const string BotToken = "ВАШ_TELEGRAM_BOT_TOKEN";

        public NotificationHostedService(
            ILogger<NotificationHostedService> logger,
            IServiceProvider services,
            IHttpClientFactory httpFactory)
        {
            _logger = logger;
            _services = services;
            _httpFactory = httpFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<EventHubDbContext>();

                    var now = DateTime.UtcNow;
                    var target = now.AddHours(24);
                    var windowEnd = now.AddHours(25); 

                    var evs = await db.Events
                        .Where(e =>
                            e.StartTime >= target &&
                            e.StartTime < windowEnd)
                        .ToListAsync(stoppingToken);

                    foreach (var ev in evs)
                    {
                        var usersToNotify = await db.Users
                            .Where(u => u.NotifyBeforeEvent && u.TelegramId != null)
                            .ToListAsync(stoppingToken);

                        foreach (var user in usersToNotify)
                        {
                            await SendTelegramMessageAsync(user.TelegramId!,
                                $"Напоминание: мероприятие \"{ev.Title}\" начнётся {ev.StartTime:yyyy-MM-dd HH:mm} UTC");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in notification loop");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task SendTelegramMessageAsync(string chatId, string text)
        {
            var client = _httpFactory.CreateClient();
            var url = $"https://api.telegram.org/bot{BotToken}/sendMessage";
            var payload = new
            {
                chat_id = chatId,
                text = text
            };
            var content = new StringContent(JsonSerializer.Serialize(payload),
                System.Text.Encoding.UTF8, "application/json");

            var resp = await client.PostAsync(url, content);
            if (!resp.IsSuccessStatusCode)
                _logger.LogWarning("Telegram send failed: {Status}", resp.StatusCode);
        }
    }
}
