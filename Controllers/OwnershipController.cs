using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using EventHub.Data;
using EventHub.Models;
using EventHub.Models.DTOs;
using System.Text.Json;

namespace EventHub.Controllers
{
    [ApiController]
    [Route("api/ownership")]
    [Authorize(Roles = "Owner")]
    public class OwnershipController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly EventHubDbContext _context;
        private readonly ILogger<OwnershipController> _logger;

        public OwnershipController(
            UserManager<User> userManager,
            EventHubDbContext context,
            ILogger<OwnershipController> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        [HttpPost("request-transfer")]
        public async Task<IActionResult> RequestTransfer([FromBody] TransferOwnershipRequestDto request)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var currentUser = await _userManager.FindByIdAsync(currentUserId.ToString());
                var newOwner = await _userManager.FindByIdAsync(request.NewOwnerId.ToString());

                if (currentUser == null || newOwner == null)
                {
                    return NotFound("User not found");
                }

                // Проверяем, что у текущего пользователя есть привязанный телеграм
                if (!currentUser.IsTelegramVerified || currentUser.TelegramId == null)
                {
                    return BadRequest("You must have a verified Telegram account to transfer ownership");
                }

                // Проверяем, что новый владелец существует и не является текущим владельцем
                if (currentUserId == request.NewOwnerId)
                {
                    return BadRequest("You cannot transfer ownership to yourself");
                }

                // Генерируем код верификации (6 цифр)
                var verificationCode = GenerateVerificationCode();
                
                // Отправляем код в телеграм (здесь должна быть интеграция с Telegram Bot API)
                await SendTelegramCode(currentUser.TelegramId.Value, verificationCode);

                // Логируем запрос
                await LogActivity(currentUserId, "OWNERSHIP_TRANSFER_REQUESTED", "User", request.NewOwnerId, 
                    $"Requested ownership transfer to user {newOwner.Name} ({newOwner.Email})");

                return Ok(new TransferOwnershipResponseDto
                {
                    Success = true,
                    Message = "Verification code sent to your Telegram",
                    VerificationCode = verificationCode // В продакшене не отправляем код в ответе
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting ownership transfer");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("confirm-transfer")]
        public async Task<IActionResult> ConfirmTransfer([FromBody] TransferOwnershipDto request)
        {
            try
            {
                var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var currentUser = await _userManager.FindByIdAsync(currentUserId.ToString());
                var newOwner = await _userManager.FindByIdAsync(request.NewOwnerId.ToString());

                if (currentUser == null || newOwner == null)
                {
                    return NotFound("User not found");
                }

                // Проверяем код верификации (в реальном приложении код должен быть сохранен в сессии или кэше)
                if (!ValidateVerificationCode(request.VerificationCode))
                {
                    return BadRequest("Invalid verification code");
                }

                // Выполняем передачу прав
                await TransferOwnership(currentUser, newOwner);

                // Логируем успешную передачу
                await LogActivity(currentUserId, "OWNERSHIP_TRANSFERRED", "User", request.NewOwnerId, 
                    $"Successfully transferred ownership to {newOwner.Name} ({newOwner.Email})");

                return Ok(new { Success = true, Message = "Ownership transferred successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming ownership transfer");
                return StatusCode(500, "Internal server error");
            }
        }

        private string GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private bool ValidateVerificationCode(string code)
        {
            // В реальном приложении здесь должна быть проверка кода из кэша/сессии
            // Пока что просто проверяем формат
            return code.Length == 6 && code.All(char.IsDigit);
        }

        private async Task SendTelegramCode(long telegramId, string code)
        {
            // Здесь должна быть интеграция с Telegram Bot API
            // Пока что просто логируем
            _logger.LogInformation($"Sending verification code {code} to Telegram ID {telegramId}");
            
            // В реальном приложении:
            // var botToken = "YOUR_BOT_TOKEN";
            // var message = $"Your verification code for ownership transfer: {code}";
            // await SendTelegramMessage(botToken, telegramId, message);
        }

        private async Task TransferOwnership(User currentOwner, User newOwner)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Удаляем роль Owner у текущего владельца
                await _userManager.RemoveFromRoleAsync(currentOwner, "Owner");
                
                // Добавляем роль SeniorAdmin текущему владельцу
                await _userManager.AddToRoleAsync(currentOwner, "SeniorAdmin");

                // Удаляем все роли у нового владельца
                var newOwnerRoles = await _userManager.GetRolesAsync(newOwner);
                await _userManager.RemoveFromRolesAsync(newOwner, newOwnerRoles);
                
                // Добавляем роль Owner новому владельцу
                await _userManager.AddToRoleAsync(newOwner, "Owner");

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task LogActivity(int userId, string action, string entityType, int? entityId, string details)
        {
            var log = new ActivityLog
            {
                UserId = userId,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                Details = details,
                UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
                Timestamp = DateTime.UtcNow
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
} 