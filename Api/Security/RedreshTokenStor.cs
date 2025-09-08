using Domin;
using Infra.Persist;

namespace Api.Security
{
    public interface IRefreshTokenStore
    {
        void Save(RefreshToken token);
        RefreshToken? Get(string tokenHash);
        void Revoke(RefreshToken token, string? replacedByTokenHash, string byIp);
        bool IsAccessJtiRevokedAsync(string accessJti);
        void RevokeAllForUser(string userId, string byIp);
    }

    public class RefreshTokenStore : IRefreshTokenStore
    {
        private readonly BoooooooooksDBContext _context;
        public RefreshTokenStore(BoooooooooksDBContext context)
        {
            _context = context;
        }

        public void Save(RefreshToken token)
        {
            _context.RefreshTokens.Add(token);
            _context.SaveChanges();
        }

        public RefreshToken? Get(string tokenHash)
        {
            return _context.RefreshTokens.Where(r => r.TokenHash == tokenHash).FirstOrDefault();
        }

        public void Revoke(RefreshToken token, string? replacedByTokenHash, string byIp)
        {
            token.RevokedAtUtc = DateTime.UtcNow;
            token.RevokedByIp = byIp;
            token.ReplacedByTokenHash = replacedByTokenHash;
            _context.RefreshTokens.Update(token);
            _context.SaveChanges();
        }

        public void RevokeAllForUser(string userId, string byIp)
        {
            foreach (var t in _context.RefreshTokens.Where(x => x.UserId == userId && x.IsActive).AsEnumerable())
            {
                t.RevokedAtUtc = DateTime.UtcNow;
                t.RevokedByIp = byIp;
                _context.RefreshTokens.Update(t);
            }
            _context.SaveChanges();
        }

        public bool IsAccessJtiRevokedAsync(string accessJti)
        {
            return _context.RefreshTokens.Where(r => r.TokenHash == accessJti && (DateTime.UtcNow >= r.ExpiresAtUtc || r.RevokedAtUtc != null)).Any();
        }
    }
}
