<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
﻿// Controllers/BotController.cs
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
<<<<<<< HEAD
using Telegram.Bot;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;
=======
<<<<<<< HEAD
using Telegram.Bot;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;
=======
<<<<<<< HEAD
using Telegram.Bot;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;
=======
=======
﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;           // ← добавлено
using Microsoft.EntityFrameworkCore;
>>>>>>> 573f3e0705c1e3252b4cddd7cfc9446f4bee2932
using Telegram.Bot;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;
<<<<<<< HEAD
=======
using EventHub.Data;                            // ← добавлено
using AppUser = EventHub.Models.User;           // ← псевдоним для вашего User
using TgUpdate = Telegram.Bot.Types.Update;     // ← псевдоним для Update
>>>>>>> 573f3e0705c1e3252b4cddd7cfc9446f4bee2932
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9

namespace EventHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BotController : ControllerBase
    {
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
        readonly ITelegramBotClient    _bot;
        readonly EventHubDbContext    _db;
        readonly UserManager<AppUser> _users;
        readonly string               _secret;
        readonly string               _baseUrl;
        readonly string               _token;
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
=======
        private readonly ITelegramBotClient _bot;
        private readonly EventHubDbContext _db;
        private readonly UserManager<AppUser> _userManager;
        private readonly string _webhookSecret;
        private readonly string _webhookBaseUrl;
        private readonly string _botToken;
>>>>>>> 573f3e0705c1e3252b4cddd7cfc9446f4bee2932
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9

        public BotController(
            ITelegramBotClient bot,
            EventHubDbContext db,
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
            UserManager<AppUser> users,
            IConfiguration     cfg)
        {
            _bot    = bot;
            _db     = db;
            _users  = users;
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
            _secret = Environment.GetEnvironmentVariable("TELEGRAM_WEBHOOK_SECRET")!;
            _baseUrl= Environment.GetEnvironmentVariable("TELEGRAM_WEBHOOK_URL")!;
            _token  = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN")!;
        }

        // call once from your browser to register webhook
        [HttpGet("setup-webhook")]
        public async Task<IActionResult> SetupWebhook()
        {
            try
            {
                if (string.IsNullOrEmpty(_token))
                {
                    return BadRequest(new { 
                        error = "Bot token not configured", 
                        message = "Please set TELEGRAM_BOT_TOKEN in .env file" 
                    });
                }

                var hook = $"{_baseUrl.TrimEnd('/')}/api/Bot/{_secret}/webhook";
                
                // For development, just return the webhook URL without setting it
                if (_baseUrl.Contains("localhost"))
                {
                    return Ok(new { 
                        webhook = hook, 
                        message = "Development mode - webhook URL generated but not set (requires HTTPS for Telegram)",
                        note = "Use ngrok or similar service to expose localhost via HTTPS"
                    });
                }

                await _bot.SetWebhookAsync(hook);
                return Ok(new { webhook = hook, message = "Webhook set successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { 
                    error = "Failed to set webhook", 
                    message = ex.Message,
                    details = ex.ToString()
                });
            }
        }

        // Test endpoint to check configuration
        [HttpGet("test-config")]
        public IActionResult TestConfig()
        {
            return Ok(new
            {
                botToken = string.IsNullOrEmpty(_token) ? "NOT_SET" : "SET",
                webhookSecret = string.IsNullOrEmpty(_secret) ? "NOT_SET" : "SET", 
                baseUrl = string.IsNullOrEmpty(_baseUrl) ? "NOT_SET" : _baseUrl,
                webhookUrl = $"{_baseUrl?.TrimEnd('/')}/api/Bot/{_secret}/webhook",
                isLocalhost = _baseUrl?.Contains("localhost") ?? false
            });
        }
        
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
<<<<<<< HEAD
            
            // Проверяем и сбрасываем верификацию если TelegramId отсутствует
            if (user.IsTelegramVerified && (user.TelegramId == null || user.TelegramId == 0))
            {
                user.IsTelegramVerified = false;
                await _users.UpdateAsync(user);
            }
            
=======
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
            if (user.IsTelegramVerified)
                return BadRequest(new { message = "You are already verified." });
            if (user.TelegramId == null)
                return BadRequest(new { message = "First link Telegram (GET /api/User/link-telegram)." });

            var code = new Random().Next(100000, 999999);
            user.TelegramCode = code;
            await _users.UpdateAsync(user);

            await _bot.SendTextMessageAsync(
                chatId: user.TelegramId.Value,
                text: $"Your verification code: {code}. Send it here."
            );
            return Ok(new { message = "Code sent to Telegram." });
        }

        // 3) All incoming updates from Telegram come here
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
=======
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
=======
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

>>>>>>> 573f3e0705c1e3252b4cddd7cfc9446f4bee2932
            if (user.TelegramId == null)
                return BadRequest(new { message = "Сначала привяжите Telegram (GET /api/User/link-telegram)." });

            var code = new Random().Next(100000, 999999);
            user.TelegramCode = code;
<<<<<<< HEAD
            await _users.UpdateAsync(user);
=======
            await _userManager.UpdateAsync(user);
>>>>>>> 573f3e0705c1e3252b4cddd7cfc9446f4bee2932

            await _bot.SendTextMessageAsync(
                chatId: user.TelegramId.Value,
                text: $"Ваш проверочный код: {code}. Отправьте его сюда."
            );
<<<<<<< HEAD
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
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
        [HttpPost("{secret}/webhook"), AllowAnonymous]
        public async Task<IActionResult> Webhook(string secret)
        {
            if (secret != _secret)
                return Unauthorized();

<<<<<<< HEAD
            // Parse JSON manually
=======
<<<<<<< HEAD
            // Parse JSON manually
=======
<<<<<<< HEAD
            // Parse JSON manually
=======
            // Парсим JSON руками
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
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

<<<<<<< HEAD
            // 1) user wrote "/start {userId}" — binding + code generation
=======
<<<<<<< HEAD
            // 1) user wrote "/start {userId}" — binding + code generation
=======
<<<<<<< HEAD
            // 1) user wrote "/start {userId}" — binding + code generation
=======
            // 1) пользователь написал "/start {userId}" — привязка + генерация кода
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
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
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
                            text: $"Hello! To complete verification, enter this code on the website:\n\n<b>{user.TelegramCode}</b>",
                            parseMode: ParseMode.Html
                        );
                    }
                }
            }

            // 2) ignore everything else — code verification will be through API
            return Ok();
        }
<<<<<<< HEAD
=======
<<<<<<< HEAD
=======
=======
                            text: $"Привет! Чтобы завершить верификацию, введи этот код на сайте:\n\n<b>{user.TelegramCode}</b>",
                            parseMode: ParseMode.Html
                        );
=======

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
>>>>>>> 573f3e0705c1e3252b4cddd7cfc9446f4bee2932
                    }
                }
                return Ok();
            }
<<<<<<< HEAD
=======

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
>>>>>>> 573f3e0705c1e3252b4cddd7cfc9446f4bee2932

            // 2) игнорируем всё остальное — проверка кода будет через API
            return Ok();
        }
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
>>>>>>> 3a88c209cf9953d8682fb13bab450d4d50f74bc9
    }
}
