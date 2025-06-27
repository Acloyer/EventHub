// File: Services/NotificationHostedService.cs
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventHub.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

namespace EventHub.Services
{
    public class NotificationHostedService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ITelegramBotClient _bot;

        public NotificationHostedService(IServiceProvider services, ITelegramBotClient bot)
        {
            _services = services;
            _bot = bot;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<EventHubDbContext>();

                var now = DateTime.UtcNow;
                var upcomingTime = now.AddMinutes(30);

                var due = db.PlannedEvents
                    .Where(pe =>
                        pe.User.NotifyBeforeEvent &&
                        pe.User.IsTelegramVerified &&
                        pe.User.TelegramId.HasValue &&
                        pe.Event.StartDate >= now &&
                        pe.Event.StartDate <= upcomingTime)
                    .Include(pe => pe.User)
                    .Include(pe => pe.Event)
                    .ToList();

                foreach (var pe in due)
                {
                    var chatId = pe.User.TelegramId.Value;
                    var text = $"Reminder: event \"{pe.Event.Title}\" starts at {pe.Event.StartDate:HH:mm}";

                    await _bot.SendTextMessageAsync(
                        chatId: chatId,
                        text: text,
                        cancellationToken: stoppingToken);
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}