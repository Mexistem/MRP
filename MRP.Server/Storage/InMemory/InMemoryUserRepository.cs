using System;
using System.Collections.Generic;
using System.Linq;
using MRP.Server.Models;

namespace MRP.Server.Storage.InMemory
{
    public sealed class InMemoryUserRepository : IUserRepository
    {
        private readonly Dictionary<string, User> _users =
            new(StringComparer.OrdinalIgnoreCase);

        public bool Exists(string username)
            => _users.ContainsKey(username);

        public User? Get(string username)
            => _users.TryGetValue(username, out var user) ? user : null;

        public void Add(User user)
            => _users[user.Username] = user;

        public IEnumerable<User> GetAll()
        {
            return _users.Values.ToList();
        }

        public void Delete(string username)
        {
            _users.Remove(username);
        }
    }
}

