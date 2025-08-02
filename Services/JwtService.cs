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
<<<<<<< HEAD
                throw new InvalidOperationException("JWT Key is not set in configuration!");
=======
<<<<<<< HEAD
                throw new InvalidOperationException("JWT Key is not set in configuration!");
=======
                throw new InvalidOperationException("JWT Key не задан в конфигурации!");
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c

            _keyBytes = Encoding.UTF8.GetBytes(_options.Key);
        }

        public string GenerateToken(User user, IList<string>? roles)
        {
<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
            // if roles == null, replace with empty list
            roles ??= Array.Empty<string>();

            // continue working with roles as usual
<<<<<<< HEAD
=======
=======
            // если roles == null, заменим на пустой список
            roles ??= Array.Empty<string>();

            // дальше работаем с roles как обычно
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
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

<<<<<<< HEAD
=======
<<<<<<< HEAD
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
        /// <summary>
        /// Generates a temporary impersonation token for the Owner to access another user's account
        /// </summary>
        public string GenerateImpersonationToken(User targetUser, IList<string> targetUserRoles, int impersonatorId)
        {
            targetUserRoles ??= Array.Empty<string>();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, targetUser.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("id", targetUser.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, targetUser.Id.ToString()),
                new Claim("impersonator_id", impersonatorId.ToString()), // Track who is impersonating
                new Claim("is_impersonation", "true") // Mark this as an impersonation token
            };

            foreach (var role in targetUserRoles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            // Impersonation tokens have a shorter expiration time for security
            var jwt = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30), // 30 minutes for impersonation tokens
                signingCredentials: creds
            );
            
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
<<<<<<< HEAD
=======
=======
>>>>>>> eb9d22584f7060235eadd9b35925603cfec8fc17
>>>>>>> bd47b2d28e579dbce8337936872728fa34fdfe4c
    }
}
