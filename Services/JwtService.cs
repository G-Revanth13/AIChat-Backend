using AIChatBot.Data;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;

namespace AIChatBot.Services
{
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _settings;

        public JwtService(IOptions<JwtSettings> options)
        {
            _settings = options.Value;
        }

        public string GenerateToken(string userId, string email)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            if (string.IsNullOrEmpty(_settings.Secret))
            {
                throw new Exception("JWT Secret is missing in JwtService");
            }

            var keyBytes = Encoding.UTF8.GetBytes(_settings.Secret);

            // HMAC-SHA256 requires a key size of at least 256 bits (32 bytes).
            // If the configured key is shorter, expand it by computing its SHA-256 hash to ensure minimum length.
            if (keyBytes.Length < 32)
            {
                using var sha = SHA256.Create();
                keyBytes = sha.ComputeHash(keyBytes);
            }
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes > 0 ? _settings.ExpiryMinutes : 60),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}