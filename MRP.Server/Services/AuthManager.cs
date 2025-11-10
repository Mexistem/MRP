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
        private readonly Dictionary<string, TokenInfo> _tokens = new(StringComparer.OrdinalIgnoreCase);
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

            string tokenString = $"{user.Username.ToLower()}-mrpToken";

            var tokenInfo = new TokenInfo
            {
                Token = tokenString,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30)
            };

            if(!_tokens.ContainsKey(user.Username))
            {
                _tokens[user.Username] = tokenInfo;
            }

            return _tokens[user.Username].Token;
        }

        public TokenInfo? GetTokenInfo(string username)
        {
            _tokens.TryGetValue(username, out TokenInfo? info);
            return info;
        }
    }
}
