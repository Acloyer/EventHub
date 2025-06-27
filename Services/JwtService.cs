using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EventHub.Models;
using EventHub.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using EventHub.Data;  // JwtOptions

namespace EventHub.Services
{
    public class JwtService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly JwtOptions _options;
        private readonly byte[] _keyBytes;

        public JwtService(IOptions<JwtOptions> opts)
        {
            _options = opts.Value;
            if (string.IsNullOrWhiteSpace(_options.Key))
                throw new InvalidOperationException("JWT Key не задан в конфигурации!");

            _keyBytes = Encoding.UTF8.GetBytes(_options.Key);
        }

        public string GenerateToken(User user, IList<string>? roles)
        {
            // если roles == null, заменим на пустой список
            roles ??= Array.Empty<string>();

            // дальше работаем с roles как обычно
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("id", user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var jwt = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_options.ExpireMinutes),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

    }
}
