using DigitalProject.Models;
using DigitalProject.Response;
using DigitalProject.Security;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DigitalProject.Security
{
    public class JwtHelper : IJwtHelper
    {
        private readonly IConfiguration _configuration;

        public JwtHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public AuthResponse GenerateToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtTokenSettings");

            var issuerSigningKey = jwtSettings["IssuerSigningKey"]
                ?? throw new InvalidOperationException("IssuerSigningKey is not configured");
            var issuer = jwtSettings["Issuer"]
                ?? throw new InvalidOperationException("Issuer is not configured");
            var audience = jwtSettings["Audience"]
                ?? throw new InvalidOperationException("Audience is not configured");
            var expireUnitStr = jwtSettings["ExpirationMinutes"]
                ?? throw new InvalidOperationException("ExpirationMinutes is not configured");
            var refreshExpireDaysStr = jwtSettings["RefreshTokenExpirationDays"]
                ?? throw new InvalidOperationException("RefreshTokenExpirationDays is not configured");

            var expireInMin = int.Parse(expireUnitStr);
            var refreshExpireInDays = int.Parse(refreshExpireDaysStr);

            // 驗證密鑰長度，HMAC-SHA256 建議至少 32 bytes
            if (Encoding.UTF8.GetBytes(issuerSigningKey).Length < 32)
                throw new ArgumentException("IssuerSigningKey must be at least 32 bytes for HMAC-SHA256");

            // ── Login Token (JWT) ──────────────────────────────────────────
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(issuerSigningKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.UtcNow.AddMinutes(expireInMin);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name,user.DisplayName),
            };
            //從 UserRoles 取得所有 Role Code 加入 Claims
            foreach (var userRole in user.UserRoles)
                claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Code));

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiry,
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            var encodedToken = tokenHandler.WriteToken(token);

            // ── Refresh Token ──────────────────────────────────────────────
            // Refresh Token 不需要攜帶 Claims，只需要是無法猜測的隨機字串
            // 用 RandomNumberGenerator 產生 256-bit 隨機值，再轉成 Base64
            var refreshToken = GenerateRefreshToken();

            // ── 組合回傳 ───────────────────────────────────────────────────
            return new AuthResponse
            {
                Id = user.Id,
                Token = encodedToken,
                RefreshToken = refreshToken,
                Email = user.Email,
                DisplayName = user.DisplayName,
                AvatarUrl = user.AvatarUrl,
                Role = string.Join(",",
                user.UserRoles.Select(ur => ur.Role.Code))
            };
        }

        // ── 私有輔助方法 ───────────────────────────────────────────────────
        private static string GenerateRefreshToken()
        {
            // 產生 256-bit (32 bytes) 的密碼學安全隨機值
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}