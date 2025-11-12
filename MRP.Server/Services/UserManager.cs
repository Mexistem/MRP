using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MRP.Server.Models;

namespace MRP.Server.Services
{
    public class UserManager
    {
        private readonly List<User> _users = [];
        public UserManager() { }

        private void AddUser(User user)
        {
            if (_users.Any(u => u.Username.Equals(user.Username, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException("A user with this username already exists");
            }

            _users.Add(user);
        }

        public void Register(string username, string password)
        {
            var user = new User(username, password);
            AddUser(user);
        }

        public User? GetUser(string username)
        {
            return _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        public void RegisterAdmin(string username, string password)
        {
            var admin = new Admin(username, password);
            AddUser(admin);
        }
    }
}
