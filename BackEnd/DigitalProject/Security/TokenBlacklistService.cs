using System.Collections.Concurrent;

namespace DigitalProject.Security
{
    public class TokenBlacklistService : ITokenBlacklistService
    {
        // Key: token 字串, Value: 該 token 的過期時間
        private readonly ConcurrentDictionary<string, DateTime> _blacklist = new();

        public void Blacklist(string token, DateTime expiry)
        {
            _blacklist[token] = expiry;
            PurgeExpired();
        }

        public bool IsBlacklisted(string token)
        {
            if (_blacklist.TryGetValue(token, out var expiry))
            {
                // 如果 token 已自然過期，順便清掉
                if (expiry < DateTime.UtcNow)
                {
                    _blacklist.TryRemove(token, out _);
                    return false;
                }
                return true;
            }
            return false;
        }

        // 清除所有已過期的 token，避免記憶體無限增長
        private void PurgeExpired()
        {
            var expired = _blacklist
                .Where(kv => kv.Value < DateTime.UtcNow)
                .Select(kv => kv.Key)
                .ToList();

            foreach (var key in expired)
                _blacklist.TryRemove(key, out _);
        }
    }
}
