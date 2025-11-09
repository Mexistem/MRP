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
        private readonly List<User> _users = new();
        public UserManager() { }

        public void AddUser(User user)
        {
            if (_users.Any(u => u.Username == user.Username))
            {
                throw new InvalidOperationException("A user with this username already exists");
            }

            _users.Add(user);
        }
    }
}
