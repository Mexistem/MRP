using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

            string tokenString = GenerateToken();

            var tokenInfo = new TokenInfo
            {
                Token = tokenString,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30)
            };

            _tokens[user.Username] = tokenInfo;

            return tokenString;
        }

        public void Logout(string username)
        {

        }

        private static string GenerateToken()
        {
            byte[] bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes);
        }

        public void ValidateToken(string username, string token)
        {
            if (!_tokens.TryGetValue(username, out var info))
            {
                throw new UnauthorizedAccessException("No active token found");
            }

            if(info.Token != token)
            {
                throw new UnauthorizedAccessException("Invalid Token");
            }
            if (info.ExpiresAt <= DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Token expired");
            }
        }

        public TokenInfo? GetTokenInfo(string username)
        {
            _tokens.TryGetValue(username, out TokenInfo? info);
            return info;
        }
    }
}
