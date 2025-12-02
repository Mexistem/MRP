using MRP.Server.Models;

public interface IUserRepository
{
    bool Exists(string username);
    User? Get(string username);
    void Add(User user);
    IEnumerable<User> GetAll();
    void Delete(string username);
}