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
        [HttpPost("{secret}/webhook"), AllowAnonymous]
        public async Task<IActionResult> Webhook(string secret)
        {
            if (secret != _secret)
                return Unauthorized();

            // Parse JSON manually
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

            // 1) user wrote "/start {userId}" — binding + code generation
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
                            text: $"Hello! To complete verification, enter this code on the website:\n\n<b>{user.TelegramCode}</b>",
                            parseMode: ParseMode.Html
                        );
                    }
                }
            }

            // 2) ignore everything else — code verification will be through API
            return Ok();
        }
    }
}
