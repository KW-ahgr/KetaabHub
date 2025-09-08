using Domin;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Api.Security
{
    public interface IJwtService
    {
        (string accessToken, DateTime expiresAtUtc, string jti) CreateAccessToken(string userId, string username, IEnumerable<string> roles);
        (string refreshTokenPlain, RefreshToken entity) CreateRefreshToken(string userId, string createdByIp);
        string Hash(string input);
    }

    public class JwtService : IJwtService
    {
        private readonly JwtOptions _opt;
        private readonly SymmetricSecurityKey _key;

        public JwtService(Microsoft.Extensions.Options.IOptions<JwtOptions> opt)
        {
            _opt = opt.Value ?? throw new ArgumentNullException(nameof(opt));
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key));
        }

        public (string accessToken, DateTime expiresAtUtc, string jti) CreateAccessToken(string userId, string username, IEnumerable<string> roles)
        {
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(_opt.AccessTokenMinutes);
            var jti = Guid.NewGuid().ToString("N");

            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.UniqueName, username),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, username),
            new(JwtRegisteredClaimNames.Jti, jti),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var token = new JwtSecurityToken(
                issuer: _opt.Issuer,
                audience: _opt.Audience,
                claims: claims,
                notBefore: now,
                expires: expires,
                signingCredentials: creds);

            var access = new JwtSecurityTokenHandler().WriteToken(token);
            return (access, expires, jti);
        }

        public (string refreshTokenPlain, RefreshToken entity) CreateRefreshToken(string userId, string createdByIp)
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            var plain = Convert.ToBase64String(bytes);

            var entity = new RefreshToken
            {
                TokenHash = Hash(plain),
                UserId = userId,
                CreatedAtUtc = DateTime.UtcNow,
                CreatedByIp = createdByIp,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(_opt.RefreshTokenDays)
            };
            return (plain, entity);
        }

        public string Hash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }
    }

    public static class Hsh
    {
        public static (string Hash, string Salt) HashPassword(string password)
        {
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 100000, HashAlgorithmName.SHA256))
            {
                byte[] hashBytes = pbkdf2.GetBytes(32);
                string hash = Convert.ToBase64String(hashBytes);
                string salt = Convert.ToBase64String(saltBytes);
                return (hash, salt);
            }
        }

        public static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            byte[] saltBytes = Convert.FromBase64String(storedSalt);
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, 100000, HashAlgorithmName.SHA256))
            {
                byte[] hashBytes = pbkdf2.GetBytes(32);
                string computedHash = Convert.ToBase64String(hashBytes);
                return computedHash == storedHash;
            }
        }
    }

    /// <summary>
    /// JwtOptions
    /// </summary>
    public sealed class JwtOptions
    {
        /// <summary> Issuer </summary>
        public string Issuer { get; set; } = default!;
        /// <summary> Audience </summary>
        public string Audience { get; set; } = default!;
        /// <summary> AccessTokenMinutes - default = 15 </summary>
        public int AccessTokenMinutes { get; set; } = 15;
        /// <summary> RefreshTokenDays - default = 14 </summary>
        public int RefreshTokenDays { get; set; } = 14;
        /// <summary> Key </summary>
        public string Key { get; set; } = default!;
    }

    /// <summary>
    /// LoginRequest
    /// </summary>
    /// <param name="Username"></param>
    /// <param name="Password"></param>
    public record LoginRequest(string Username, string Password);

    /// <summary>
    /// LoginResponse
    /// </summary>
    /// <param name="AccessToken"></param>
    /// <param name="ExpiresAtUtc"></param>
    /// <param name="RefreshToken"></param>
    public record LoginResponse(string AccessToken, DateTime ExpiresAtUtc, string RefreshToken);

    /// <summary>
    /// RefreshRequest
    /// </summary>
    /// <param name="RefreshToken"></param>
    public record RefreshRequest(string RefreshToken);

    /// <summary>
    /// RevokeRequest
    /// </summary>
    /// <param name="RefreshToken"></param>
    public record RevokeRequest(string RefreshToken);

    /// <summary>
    /// CreateUserRequest
    /// </summary>
    public record CreateUserRequest
    {
        /// <summary>
        /// SureName
        /// </summary>
        public string SureName { get; set; } = default!;

        /// <summary>
        /// Username
        /// </summary>
        public required string Username { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public required string Password { get; set; }

        /// <summary>
        /// Role
        /// </summary>
        public string Role { get; set; } = "User";
    }

}
