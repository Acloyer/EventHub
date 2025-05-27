using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using BCrypt.Net;
using EventHub.Data;
using EventHub.Models;
using EventHub.Models.Dto;

namespace EventHub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly EventHubDbContext _db;
        private readonly IConfiguration _config;

        public AuthController(EventHubDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("User already exists.");

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var token = GenerateJwt(user);
            return Ok(new { Token = token });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials.");

            var token = GenerateJwt(user);
            return Ok(new { Token = token });
        }

        private string GenerateJwt(User user)
        {
            // 1) Считаем у этого юзера роли из базы
            var roles = _db.UserRoles
                           .Where(ur => ur.UserId == user.Id)
                           .Include(ur => ur.Role)
                           .Select(ur => ur.Role.Name)
                           .ToList();

            // 2) Собираем клаимы: sub, email и роли
            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email)
    };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // 3) Создаём токен
            var jwtSection = _config.GetSection("Jwt");
            var keyBytes = Encoding.UTF8.GetBytes(jwtSection["Key"]!);
            var creds = new SigningCredentials(
                                new SymmetricSecurityKey(keyBytes),
                                SecurityAlgorithms.HmacSha256
                             );

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                                        double.Parse(jwtSection["ExpireMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
