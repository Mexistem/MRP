using MRP.Server.Models;

namespace MRP.Server.Storage.InMemory
{
    public sealed class InMemoryTokenRepository : ITokenRepository
    {
        private readonly Dictionary<string, TokenInfo> _tokens =
        new(StringComparer.OrdinalIgnoreCase);

        public TokenInfo? GetByUsername(string username)
        {
            _tokens.TryGetValue(username, out var info);
            return info;
        }

        public string? GetUsernameByToken(string token)
        {
            foreach (var kvp in _tokens)
            {
                if (kvp.Value.Token == token)
                {
                    return kvp.Key;
                }
            }
            return null;
        }

        public void SetToken(string username, TokenInfo token)
        {
            _tokens[username] = token;
        }

        public void RemoveToken(string username)
        {
            _tokens.Remove(username);
        }

        public void RemoveExpiredTokens()
        {
            var now = DateTime.UtcNow;

            var expiredUsernames = _tokens
                .Where(kvp => kvp.Value.ExpiresAt <= now)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var username in expiredUsernames)
            {
                _tokens.Remove(username);
            }
        }
    }
}
