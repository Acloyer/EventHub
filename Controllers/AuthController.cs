using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;

using BCrypt.Net;
using EventHub.Data;
using EventHub.Models;
using EventHub.Models.DTOs;
using EventHub.Services;
using Microsoft.AspNetCore.Authorization;

namespace EventHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ApplicationDbContext db, IConfiguration config, IUserService userService, ILogger<AuthController> logger)
        {
            _db = db;
            _config = config;
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<UserDto>> Register(UserRegisterDto registerDto)
        {
            try
            {
                var result = await _userService.Register(registerDto);
                if (result == null)
                {
                    return BadRequest(new { message = "Email already exists" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, new { message = "Error during registration" });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<UserDto>> Login(UserLoginDto loginDto)
        {
            try
            {
                 // 1. Найди пользователя по email
                var user = await _db.Users
                    .Include(u => u.UserRoles)
                    .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

                if (user == null)
                    return Unauthorized(new { message = "Invalid email or password" });
                // 2. Проверка на бан
                if (user.IsBanned)
                {
                    return Unauthorized("К сожалению, пользователь забанен");
                }
                var result = await _userService.Login(loginDto);
                if (result == null)
                {
                    return Unauthorized(new { message = "Invalid email or password" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, new { message = "Error during login" });
            }
        }

        private string GenerateJwt(User user)
        {
            var roles = _db.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Include(ur => ur.Role)
                .Where(ur => ur.Role != null)
                .Select(ur => ur.Role!.Name)
                .ToList();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var jwtSection = _config.GetSection("Jwt");
            var keyBytes = Encoding.UTF8.GetBytes(jwtSection["Key"] ?? throw new InvalidOperationException("JWT Key not configured"));
            var creds = new SigningCredentials(
                new SymmetricSecurityKey(keyBytes),
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    double.Parse(jwtSection["ExpireMinutes"] ?? "60")),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class LoginRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public class RegisterRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
