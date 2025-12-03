using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using MRP.Server.Models;

namespace MRP.Server.Services
{
    public sealed class AuthManager : IAuthManager
    {
        private readonly UserManager _userManager;
        private readonly ITokenRepository _tokenRepository;
        public AuthManager(UserManager userManager, ITokenRepository tokenRepository)
        {
            _userManager = userManager;
            _tokenRepository = tokenRepository;
        }

        public string Login(string username, string password)
        {
            var user = _userManager.GetUser(username);


            if (user == null)
            {
                throw new InvalidOperationException("unknown user");
            }

            if(user.Password != User.HashPassword(username, password))
            {
                throw new UnauthorizedAccessException("Invalid password");
            }

            string tokenString = GenerateToken();

            var tokenInfo = new TokenInfo
            {
                Token = tokenString,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30)
            };

            _tokenRepository.SetToken(user.Username, tokenInfo);

            return tokenString;
        }

        public void Logout(string username)
        {
            _tokenRepository.RemoveToken(username);
        }

        private static string GenerateToken()
        {
            byte[] bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes);
        }

        public void ValidateToken(string username, string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            { 
                throw new UnauthorizedAccessException("Missing bearer token");
            }

            var info = _tokenRepository.GetByUsername(username);

            if (info is null || info.Token != token)
            {
                throw new UnauthorizedAccessException("Invalid or expired token");
            }

            if (info.ExpiresAt <= DateTime.UtcNow)
            {
                _tokenRepository.RemoveToken(username);
                throw new UnauthorizedAccessException("Invalid or expired token");
            }
        }

        public TokenInfo? GetTokenInfo(string username)
        {
            return _tokenRepository.GetByUsername(username);
        }

        public string? GetUsernameByToken(string token)
        {
            return _tokenRepository.GetUsernameByToken(token);
        }
    }
}
