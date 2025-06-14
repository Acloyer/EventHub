using EventHub.Data;
using EventHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Args;

namespace EventHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BotController : ControllerBase
    {
        private readonly EventHubDbContext _db;
        private readonly ITelegramBotClient _bot;
        private readonly IConfiguration _configuration;

        public BotController(EventHubDbContext db, ITelegramBotClient bot, IConfiguration configuration)
        {
            _db = db;
            _bot = bot;
            _configuration = configuration;
        }

        // GET: api/bot/setup-webhook
        [HttpGet("setup-webhook")]
        public async Task<IActionResult> SetupWebhook()
        {
            var webhookUrl = _configuration["TelegramBot:WebhookUrl"];
            if (string.IsNullOrEmpty(webhookUrl))
                return BadRequest("WebhookUrl not configured");

            try
            {
                // Удаляем текущий webhook (если есть)
                await _bot.DeleteWebhookAsync();

                // Устанавливаем новый webhook
                await _bot.SetWebhookAsync(webhookUrl);

                // Получаем информацию о webhook для проверки
                var webhookInfo = await _bot.GetWebhookInfoAsync();

                return Ok(new
                {
                    WebhookUrl = webhookInfo.Url,
                    LastErrorDate = webhookInfo.LastErrorDate,
                    LastErrorMessage = webhookInfo.LastErrorMessage,
                    PendingUpdateCount = webhookInfo.PendingUpdateCount
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // Webhook endpoint for Telegram updates
        [HttpPost("webhook")]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            if (update?.Message?.Text == null && update?.CallbackQuery == null)
                return Ok();

            try
            {
                if (update.Type == UpdateType.Message && update.Message?.Text != null)
                {
                    var message = update.Message;
                    if (message.Text.StartsWith("/start"))
                    {
                        var keyboard = new InlineKeyboardMarkup(new[]
                        {
                            new []
                            {
                                InlineKeyboardButton.WithCallbackData("Подтвердить аккаунт", "verify_account")
                            }
                        });

                        await _bot.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: "Добро пожаловать! Нажмите кнопку ниже, чтобы подтвердить ваш аккаунт.",
                            replyMarkup: keyboard
                        );
                    }
                }
                else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
                {
                    var callback = update.CallbackQuery;
                    if (callback.Data == "verify_account" && callback.Message != null)
                    {
                        var chatId = callback.Message.Chat.Id.ToString();
                        
                        // Генерируем код подтверждения
                        var code = new Random().Next(100000, 999999).ToString();
                        
                        // Сохраняем временный код в базе
                        var pendingVerification = new TelegramVerification
                        {
                            ChatId = chatId,
                            VerificationCode = code,
                            CreatedAt = DateTime.UtcNow
                        };
                        
                        _db.TelegramVerifications.Add(pendingVerification);
                        await _db.SaveChangesAsync();

                        await _bot.SendTextMessageAsync(
                            chatId: callback.Message.Chat.Id,
                            text: $"Ваш код подтверждения: *{code}*\n\nВведите этот код на сайте для завершения привязки аккаунта.",
                            parseMode: ParseMode.Markdown
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error but return OK to Telegram
                Console.WriteLine($"Error processing update: {ex}");
            }

            return Ok();
        }

        [Authorize]
        [HttpPost("verify/{code}")]
        public async Task<IActionResult> VerifyTelegramCode(string code)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var verification = await _db.TelegramVerifications
                .FirstOrDefaultAsync(v => v.VerificationCode == code);

            if (verification == null)
                return BadRequest("Неверный код подтверждения");

            if ((DateTime.UtcNow - verification.CreatedAt).TotalMinutes > 15)
            {
                _db.TelegramVerifications.Remove(verification);
                await _db.SaveChangesAsync();
                return BadRequest("Код подтверждения истек");
            }

            var user = await _db.Users.FindAsync(int.Parse(userId));
            if (user == null)
                return NotFound();

            user.TelegramId = verification.ChatId;
            user.IsTelegramVerified = true;

            _db.TelegramVerifications.Remove(verification);
            await _db.SaveChangesAsync();

            await _bot.SendTextMessageAsync(
                chatId: verification.ChatId,
                text: "✅ Ваш аккаунт успешно привязан к Telegram!",
                parseMode: ParseMode.Markdown
            );

            return Ok(new { message = "Telegram успешно привязан" });
        }
    }
}
