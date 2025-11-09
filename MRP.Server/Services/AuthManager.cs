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
        private readonly Dictionary<string, string> _tokens = new(StringComparer.OrdinalIgnoreCase);
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

            string token = $"{user.Username.ToLower()}-mrpToken";

            if(!_tokens.ContainsKey(user.Username))
            {
                _tokens[user.Username] = token;
            }

            return _tokens[user.Username];
        }

        public string? GetToken(string username)
        {
            _tokens.TryGetValue(username.ToLower(), out string? token);
            return token;
        }
    }
}
