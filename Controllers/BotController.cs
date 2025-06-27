using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;           // ← добавлено
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using EventHub.Data;                            // ← добавлено
using AppUser = EventHub.Models.User;           // ← псевдоним для вашего User
using TgUpdate = Telegram.Bot.Types.Update;     // ← псевдоним для Update

namespace EventHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BotController : ControllerBase
    {
        private readonly ITelegramBotClient _bot;
        private readonly EventHubDbContext _db;
        private readonly UserManager<AppUser> _userManager;
        private readonly string _webhookSecret;
        private readonly string _webhookBaseUrl;
        private readonly string _botToken;

        public BotController(
            ITelegramBotClient bot,
            EventHubDbContext db,
            UserManager<AppUser> userManager,
            IConfiguration config)
        {
            _bot           = bot;
            _db            = db;
            _userManager   = userManager;
            _webhookSecret = config["Telegram:WebhookSecret"]!;
            _webhookBaseUrl= config["Telegram:WebhookUrl"]!;
            _botToken      = config["Telegram:BotToken"]!;
        }

        [HttpGet("setup-webhook")]
        public async Task<IActionResult> SetupWebhook()
        {
            var hookUrl = $"{_webhookBaseUrl.TrimEnd('/')}/api/Bot/{_webhookSecret}/webhook";
            await _bot.SetWebhookAsync(hookUrl);
            return Ok(new { webhook = hookUrl });
        }

        [HttpGet("{secret}/webhook"), AllowAnonymous]
        public IActionResult GetWebhook(string secret, [FromQuery(Name = "crc_token")] string crcToken)
        {
            if (secret != _webhookSecret)
                return Unauthorized();

            using var hmac = new System.Security.Cryptography.HMACSHA256(
                System.Text.Encoding.UTF8.GetBytes(_botToken));
            var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(crcToken));
            return Ok(new { response_token = "sha256=" + Convert.ToBase64String(hash) });
        }

        [Authorize, HttpPost("start-verification")]
        public async Task<IActionResult> StartVerification()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            if (user.IsTelegramVerified)
                return BadRequest(new { message = "Вы уже верифицированы." });

            if (user.TelegramId == null)
                return BadRequest(new { message = "Сначала привяжите Telegram (GET /api/User/link-telegram)." });

            var code = new Random().Next(100000, 999999);
            user.TelegramCode = code;
            await _userManager.UpdateAsync(user);

            await _bot.SendTextMessageAsync(
                chatId: user.TelegramId.Value,
                text: $"Ваш проверочный код: {code}. Отправьте его сюда."
            );

            return Ok(new { message = "Код выслан в Telegram." });
        }

        [HttpPost("{secret}/webhook"), AllowAnonymous]
        public async Task<IActionResult> Webhook(string secret, [FromBody] TgUpdate update)
        {
            if (secret != _webhookSecret)
                return Unauthorized();

            if (update.Type != UpdateType.Message ||
                update.Message?.Type != MessageType.Text)
                return Ok();

            var chatId = update.Message.Chat.Id;
            var text   = update.Message.Text!.Trim();

            // Обработка /start <userId>
            if (text.StartsWith("/start"))
            {
                var parts = text.Split(' ', 2);
                if (parts.Length == 2 && int.TryParse(parts[1], out var userId))
                {
                    var user = await _db.Users.FindAsync(userId);
                    if (user?.TelegramCode != null)
                    {
                        user.TelegramId = chatId;
                        await _db.SaveChangesAsync();

                        await _bot.SendTextMessageAsync(chatId,
                            "Telegram привязан! Введите код из приложения.");
                    }
                }
                return Ok();
            }

            // Обработка проверочного кода
            if (int.TryParse(text, out var code))
            {
                var user = await _db.Users
                    .Where(u => u.TelegramId == chatId && u.TelegramCode == code)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    await _bot.SendTextMessageAsync(chatId,
                        "Неверный код. Попробуйте снова.");
                    return Ok();
                }

                user.IsTelegramVerified = true;
                user.TelegramCode       = null;
                await _db.SaveChangesAsync();

                await _bot.SendTextMessageAsync(chatId,
                    "Вы успешно верифицированы!");
            }

            return Ok();
        }
    }
}
