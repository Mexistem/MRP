using MRP.Server.Models;

namespace MRP.Server.Services
{
    public interface IAuthManager
    {
        string Login(string username, string password);
        void Logout(string username);
        TokenInfo? GetTokenInfo(string username);
        void ValidateToken(string username, string token);
        string? GetUsernameByToken(string token);
    }
}