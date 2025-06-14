using EventHub.Data;
using EventHub.Models;
using EventHub.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EventHub.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserService> _logger;

        public UserService(ApplicationDbContext context, IConfiguration configuration, ILogger<UserService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResponse?> AuthenticateAsync(string email, string password)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return null;

            if (!VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
                return null;

            // Generate JWT token
            var token = GenerateJwtToken(user);

            return new AuthResponse
            {
                Token = token,
                User = MapToDto(user)
            };
        }

        public async Task<AuthResponse?> RegisterAsync(string email, string password)
        {
            if (await _context.Users.AnyAsync(u => u.Email == email))
                return null;

            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                Email = email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                UserRoles = new List<UserRole>()
            };

            // Add default User role
            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (userRole != null)
            {
                user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = userRole.Id });
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync(); // сначала сохраняем, чтобы user.Id появился

            if (userRole != null)
            {
                user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = userRole.Id });
                await _context.SaveChangesAsync(); // теперь сохраняем роли
            }

            // Generate JWT token
            var token = GenerateJwtToken(user);

            return new AuthResponse
            {
                Token = token,
                User = MapToDto(user)
            };
        }

        public async Task<bool> AssignRoleAsync(int userId, string roleName)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return false;

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null)
                return false;

            if (user.UserRoles.Any(ur => ur.RoleId == role.Id))
                return true; // Role already assigned

            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = role.Id,
                User = user,
                Role = role
            };

            user.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveRoleAsync(int userId, string roleName)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return false;

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null)
                return false;

            var userRole = user.UserRoles.FirstOrDefault(ur => ur.RoleId == role.Id);
            if (userRole == null)
                return true; // Role wasn't assigned

            user.UserRoles.Remove(userRole);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return Array.Empty<string>();

            return user.UserRoles
                .Where(ur => ur.Role != null)
                .Select(ur => ur.Role!.Name)
                .ToArray();
        }

        public async Task<bool> IsInRoleAsync(int userId, string roleName)
        {
            var roles = await GetUserRolesAsync(userId);
            return roles.Contains(roleName);
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ToListAsync();

            return users.Select(MapToDto);
        }

        private UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                TelegramId = user.TelegramId,
                IsTelegramVerified = user.IsTelegramVerified,
                NotifyBeforeEvent = user.NotifyBeforeEvent,
                Roles = user.UserRoles?
                            .Select(ur => ur.Role.Name)
                            .ToArray()
                        ?? Array.Empty<string>(),
            };
        }

        private string GenerateJwtToken(User user)
        {
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT secret not configured"));
            var tokenHandler = new JwtSecurityTokenHandler();
            
            // Получаем роли пользователя
            var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
            
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            // Добавляем роли в claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        private bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
        {
            using var hmac = new HMACSHA512(storedSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(storedHash);
        }

        private string[] GetUserRoles(User? user)
        {
            if (user?.UserRoles == null)
                return Array.Empty<string>();

            return user.UserRoles
                .Where(ur => ur?.Role != null)
                .Select(ur => ur.Role!.Name)
                .ToArray();
        }

        private async Task<string[]> GetUserRolesFromDb(int userId)
        {
            var roles = await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == userId && ur.Role != null)
                .Select(ur => ur.Role!.Name)
                .ToListAsync();

            return roles?.ToArray() ?? Array.Empty<string>();
        }

        public async Task<UserDto?> Register(UserRegisterDto registerDto)
        {
            try
            {
                if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
                {
                    _logger.LogWarning("Registration attempt with existing email: {Email}", registerDto.Email);
                    return null;
                }

                using var hmac = new HMACSHA512();
                var user = new User
                {
                    Email = registerDto.Email,
                    PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                    PasswordSalt = hmac.Key,
                    UserRoles = new List<UserRole>()
                };

                // По умолчанию назначаем роль User
                var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
                if (userRole == null)
                {
                    _logger.LogError("Default User role not found during registration");
                    throw new Exception("Default User role not found");
                }

                var newUserRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = userRole.Id,
                    User = user,
                    Role = userRole
                };

                user.UserRoles.Add(newUserRole);
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var token = await GenerateTokenAsync(user);
                return new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Token = token,
                    Roles = new[] { "User" }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                throw;
            }
        }

        public async Task<UserDto?> Login(UserLoginDto loginDto)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

                if (user == null)
                {
                    _logger.LogWarning("Login attempt with non-existent email: {Email}", loginDto.Email);
                    return null;
                }

                using var hmac = new HMACSHA512(user.PasswordSalt);
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

                if (!computedHash.SequenceEqual(user.PasswordHash))
                {
                    _logger.LogWarning("Failed login attempt for user: {Email}", loginDto.Email);
                    return null;
                }

                var token = await GenerateTokenAsync(user);
                _logger.LogInformation("Successful login for user: {Email}", loginDto.Email);

                return new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    Token = token,
                    Roles = GetUserRoles(user)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login");
                throw;
            }
        }

        private async Task<string> GenerateTokenAsync(User user)
        {
            try
            {
                if (user == null)
                    throw new ArgumentNullException(nameof(user));

                var roles = await GetUserRolesFromDb(user.Id);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email)
                };

                claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                    _configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured")));

                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = creds,
                    Issuer = _configuration["Jwt:Issuer"],
                    Audience = _configuration["Jwt:Audience"]
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);

                _logger.LogInformation("Generated token for user: {Email}", user.Email);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating token for user: {Email}", user.Email);
                throw;
            }
        }
    }
} 