using MRP.Server.Models;
using MRP.Server.Storage.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MRP.Server.Storage.InMemory
{
    public sealed class InMemoryTokenRepository : ITokenRepository
    {
        private readonly Dictionary<string, TokenInfo> _tokens = new(StringComparer.OrdinalIgnoreCase);
        private IUserRepository? _userRepository;

        public InMemoryTokenRepository()
        {
            _userRepository = null;
        }

        public InMemoryTokenRepository(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

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
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("username is required", nameof(username));
            }

            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (_userRepository is not null && !_userRepository.Exists(username))
            {
                throw new KeyNotFoundException("User not found.");
            }

            _tokens[username] = token;
        }

        public void RemoveToken(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return;
            }

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

        public void SetDependencies(IUserRepository userRepository)
        {
            if (userRepository == null)
            {
                throw new ArgumentNullException(nameof(userRepository));
            }

            _userRepository = userRepository;
        }

    }
}
