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
        private readonly NotificationLocalizationService _localizationService;

        public NotificationHostedService(IServiceProvider services, ITelegramBotClient bot, NotificationLocalizationService localizationService)
        {
            _services = services;
            _bot = bot;
            _localizationService = localizationService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
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
                        var userLanguage = pe.User.PreferredLanguage ?? "en";
                        var text = _localizationService.GetEventReminderMessage(userLanguage, pe.Event.Title, pe.Event.StartDate);

                        await _bot.SendTextMessageAsync(
                            chatId: chatId,
                            text: text,
                            cancellationToken: stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    // Логируем ошибку, но не останавливаем сервис
                    Console.WriteLine($"NotificationHostedService error: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}