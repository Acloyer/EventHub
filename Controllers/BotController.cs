// Controllers/BotController.cs
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using EventHub.Data;
using AppUser = EventHub.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;

namespace EventHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BotController : ControllerBase
    {
        readonly ITelegramBotClient    _bot;
        readonly EventHubDbContext    _db;
        readonly UserManager<AppUser> _users;
        readonly string               _secret;
        readonly string               _baseUrl;
        readonly string               _token;

        public BotController(
            ITelegramBotClient bot,
            EventHubDbContext db,
            UserManager<AppUser> users,
            IConfiguration     cfg)
        {
            _bot    = bot;
            _db     = db;
            _users  = users;
            _secret = cfg["Telegram:WebhookSecret"]!;
            _baseUrl= cfg["Telegram:WebhookUrl"]!;
            _token  = cfg["Telegram:BotToken"]!;
        }

        // call once from your browser to register webhook
        // [HttpGet("setup-webhook")]
        // public async Task<IActionResult> SetupWebhook()
        // {
        //     var hook = $"{_baseUrl.TrimEnd('/')}/api/Bot/{_secret}/webhook";
        //     await _bot.SetWebhookAsync(hook);
        //     return Ok(new { webhook = hook });
        // }
        
        // [HttpGet("setup-webhook"), ApiExplorerSettings(IgnoreApi = true)]
        // public async Task<IActionResult> SetupWebhook()
        // {
        //     var hook = $"{_baseUrl.TrimEnd('/')}/api/Bot/{_secret}/webhook";
        //     await _bot.SetWebhookAsync(hook);
        //     return Ok(new { webhook = hook });
        // }
        
        // Telegram’s CRC check
        [HttpGet("{secret}/webhook"), AllowAnonymous]
        public IActionResult GetWebhook(string secret, [FromQuery(Name="crc_token")] string crc)
        {
            if (secret != _secret) return Unauthorized();
            using var h = new System.Security.Cryptography.HMACSHA256(
                System.Text.Encoding.UTF8.GetBytes(_token));
            var hash = h.ComputeHash(System.Text.Encoding.UTF8.GetBytes(crc));
            return Ok(new { response_token = $"sha256={Convert.ToBase64String(hash)}" });
        }

        // user clicks “Start verification” in your app
        [Authorize, HttpPost("start-verification")]
        public async Task<IActionResult> StartVerification()
        {
            var user = await _users.GetUserAsync(User);
            if (user == null) return Unauthorized();
            if (user.IsTelegramVerified)
                return BadRequest(new { message = "Вы уже верифицированы." });
            if (user.TelegramId == null)
                return BadRequest(new { message = "Сначала привяжите Telegram (GET /api/User/link-telegram)." });

            var code = new Random().Next(100000, 999999);
            user.TelegramCode = code;
            await _users.UpdateAsync(user);

            await _bot.SendTextMessageAsync(
                chatId: user.TelegramId.Value,
                text: $"Ваш проверочный код: {code}. Отправьте его сюда."
            );
            return Ok(new { message = "Код выслан в Telegram." });
        }

        
        // [HttpPost("{secret}/webhook"), AllowAnonymous]
        // public async Task<IActionResult> Webhook(string secret)
        // {
        //     if (secret != _secret)
        //         return Unauthorized();

        //     JsonDocument doc;
        //     try
        //     {
        //         doc = await JsonDocument.ParseAsync(Request.Body);
        //     }
        //     catch
        //     {
        //         return BadRequest();
        //     }

        //     var root = doc.RootElement;
        //     // если это не UpdateType.Message с текстом — ничего не делаем
        //     if (!root.TryGetProperty("message", out var msg) ||
        //         !msg.TryGetProperty("text", out var txt))
        //         return Ok();

        //     var text   = txt.GetString()!.Trim();
        //     var chatId = msg.GetProperty("chat").GetProperty("id").GetInt64();

        //     // 1) Обработка /start <userId>
        //     if (text.StartsWith("/start"))
        //     {
        //         var parts = text.Split(' ', 2);
        //         if (parts.Length == 2 && int.TryParse(parts[1], out var userId))
        //         {
        //             var user = await _db.Users.FindAsync(userId);
        //             if (user != null)
        //             {
        //                 // Привязываем TelegramId
        //                 user.TelegramId = chatId;

        //                 // Генерируем и сохраняем новый код сразу же
        //                 var code = new Random().Next(100000, 999999);
        //                 user.TelegramCode = code;
        //                 await _db.SaveChangesAsync();

        //                 // Шлём пользователю инструкцию с кодом
        //                 await _bot.SendTextMessageAsync(
        //                     chatId: chatId,
        //                     text: $"Привет! Чтобы завершить верификацию, введи на сайте этот код:\n\n<b>{code}</b>\n\n(введите его на странице Верификации)",
        //                     parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
        //                 );
        //             }
        //         }
        //         return Ok();
        //     }

        //     // 2) Обработка сообщения-кода из чата (не обязательно, можно удалить, если код вводится только на сайте)
        //     if (int.TryParse(text, out var incomingCode))
        //     {
        //         var user = await _db.Users
        //             .Where(u => u.TelegramId == chatId && u.TelegramCode == incomingCode)
        //             .FirstOrDefaultAsync();

        //         if (user == null)
        //         {
        //             await _bot.SendTextMessageAsync(
        //                 chatId: chatId,
        //                 text: "Неверный код. Попробуй ещё раз или введи код на сайте."
        //             );
        //             return Ok();
        //         }

        //         // Автоматически подтверждаем, если вы хотите сразу же
        //         user.IsTelegramVerified = true;
        //         user.TelegramCode       = null;
        //         await _db.SaveChangesAsync();

        //         await _bot.SendTextMessageAsync(
        //             chatId: chatId,
        //             text: "Спасибо! Твой аккаунт успешно верифицирован."
        //         );
        //     }

        //     return Ok();
        // }
// BotController.cs

        // 3) Все входящие update’ы от Telegram приходят сюда
        [HttpPost("{secret}/webhook"), AllowAnonymous]
        public async Task<IActionResult> Webhook(string secret)
        {
            if (secret != _secret)
                return Unauthorized();

            // Парсим JSON руками
            JsonDocument doc;
            try { doc = await JsonDocument.ParseAsync(Request.Body); }
            catch { return BadRequest(); }

            var root = doc.RootElement;
            if (!root.TryGetProperty("message", out var msg) ||
                !msg.TryGetProperty("text", out var txt)   ||
                !msg.TryGetProperty("chat", out var chat))
                return Ok();

            var text   = txt.GetString()!.Trim();
            var chatId = chat.GetProperty("id").GetInt64();

            // 1) пользователь написал "/start {userId}" — привязка + генерация кода
            if (text.StartsWith("/start"))
            {
                var parts = text.Split(' ', 2);
                if (parts.Length == 2 && int.TryParse(parts[1], out var uid))
                {
                    var user = await _db.Users.FindAsync(uid);
                    if (user != null)
                    {
                        user.TelegramId   = chatId;
                        user.TelegramCode = new Random().Next(100000, 999999);
                        await _db.SaveChangesAsync();

                        await _bot.SendTextMessageAsync(
                            chatId: chatId,
                            text: $"Привет! Чтобы завершить верификацию, введи этот код на сайте:\n\n<b>{user.TelegramCode}</b>",
                            parseMode: ParseMode.Html
                        );
                    }
                }
            }

            // 2) игнорируем всё остальное — проверка кода будет через API
            return Ok();
        }
    }
}
