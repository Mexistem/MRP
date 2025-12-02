using MRP.Server.Models;

public interface ITokenRepository
{
    TokenInfo? GetByUsername(string username);
    string? GetUsernameByToken(string token);

    void SetToken(string username, TokenInfo token);
    void RemoveToken(string username);
    void RemoveExpiredTokens();
}