using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventHub.Data;
using EventHub.Models;
using EventHub.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace EventHub.Services
{
    public class UserService : IUserService
    {
        private readonly EventHubDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly JwtService _jwtService;
        private readonly ILogger<UserService> _logger;

        public UserService(
            EventHubDbContext db,
            UserManager<User> userManager,
            JwtService jwtService,
            ILogger<UserService> logger)
        {
            _db = db;
            _userManager = userManager;
            _jwtService = jwtService;
            _logger = logger;
        }

        public async Task<AuthResponse> Register(RegisterDto model)
        {
            var already = await _userManager.FindByEmailAsync(model.Email);
            if (already != null)
                throw new InvalidOperationException("User with this email already exists");

            var user = new User {
                UserName = model.Email,
                Email    = model.Email,
                Name     = model.Name,
                IsTelegramVerified = false // Убеждаемся, что новые пользователи не верифицированы
            };
            var create = await _userManager.CreateAsync(user, model.Password);
            if (!create.Succeeded)
                throw new InvalidOperationException("Can't create a user: " +
                    string.Join("; ", create.Errors.Select(e => e.Description)));
                    
            await _userManager.AddToRoleAsync(user, "User");

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtService.GenerateToken(user, roles);

            return new AuthResponse {
                Token = token,
                User  = new UserDto(user) {
                    Roles = roles.ToList()
                }
            };
        }


        public async Task<AuthResponse> Login(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                throw new InvalidOperationException("Invalid email or password");

            if (user.IsBanned)
                throw new InvalidOperationException("User is banned");

            // Проверка и сброс Telegram верификации если необходимо
            await ValidateAndResetTelegramVerification(user);

            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtService.GenerateToken(user, roles);

            return new AuthResponse
            {
                Token = token,
                User = new UserDto(user)
                {
                    Roles = roles.ToList()
                }
            };
        }

        public Task<bool> UserExists(string email)
            => Task.FromResult(_userManager.FindByEmailAsync(email) != null);

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;
            var res = await _userManager.DeleteAsync(user);
            return res.Succeeded;
        }

        /// <summary>
        /// Проверяет и сбрасывает Telegram верификацию если TelegramId отсутствует
        /// </summary>
        public async Task ValidateAndResetTelegramVerification(User user)
        {
            if (user.IsTelegramVerified && (user.TelegramId == null || user.TelegramId == 0))
            {
                user.IsTelegramVerified = false;
                await _userManager.UpdateAsync(user);
                _logger.LogInformation($"Reset Telegram verification for user {user.Email} due to missing TelegramId");
            }
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var all = new List<UserDto>();
            var users = await _userManager.Users.ToListAsync();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                all.Add(new UserDto {
                    Id                = u.Id,
                    Email             = u.Email,
                    Name              = u.Name,
                    Roles             = roles.ToList(),
                    TelegramId        = u.TelegramId,
                    IsTelegramVerified= u.IsTelegramVerified,
                    NotifyBeforeEvent = u.NotifyBeforeEvent
                });
            }
            return all;
        }
    }
}
