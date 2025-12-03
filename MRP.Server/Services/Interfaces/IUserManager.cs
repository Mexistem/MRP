using MRP.Server.Models;

namespace MRP.Server.Services
{
    public interface IUserManager
    {
        void Register(string username, string password);
        void RegisterAdmin(string username, string password);
        User? GetUser(string username);
        IEnumerable<User> GetAllUsers();
        void DeleteUser(string username);
        bool IsAdmin(string username);
    }
}