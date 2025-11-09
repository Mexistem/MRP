using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MRP.Server.Models;

namespace MRP.Server.Services
{
    public class AuthManager
    {
        private readonly UserManager _userManager;
        public AuthManager(UserManager userManager)
        {
            _userManager = userManager;
        }

        public string Login(string username, string password)
        {
            var user = _userManager.GetUser(username);


            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            if(user.Password != User.HashPassword(password))
            {
                throw new UnauthorizedAccessException("Invalid password");
            }

            return $"{user.Username.ToLower()}-mrpToken";
        }
    }
}
