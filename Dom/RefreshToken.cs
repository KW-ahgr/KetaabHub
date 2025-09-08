using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domin
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string TokenHash { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string CreatedByIp { get; set; } = default!;
        public DateTime? RevokedAtUtc { get; set; }
        public string? RevokedByIp { get; set; }
        public string? ReplacedByTokenHash { get; set; }
        public bool IsRevoked => RevokedAtUtc.HasValue;
        public bool IsExpired => DateTime.UtcNow >= ExpiresAtUtc;
        public bool IsActive => !IsRevoked && !IsExpired;
        public string? AccessJti { get; set; }
    }
}
