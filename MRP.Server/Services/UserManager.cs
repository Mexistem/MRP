using MRP.Server.Models;
using MRP.Server.Validation;

namespace MRP.Server.Services
{
    public sealed class UserManager : IUserManager
    {
        private readonly IUserRepository _userRepository;
        public UserManager(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        private void AddUser(User user)
        {
            if (_userRepository.Exists(user.Username))
            {
                throw new InvalidOperationException("A user with this username already exists");
            }

            _userRepository.Add(user);
        }

        public void Register(string username, string password)
        {
            UserValidator.ValidateUsername(username);
            PasswordValidator.ValidatePassword(password, username);

            var user = new User(username, password);

            AddUser(user);
        }

        public User? GetUser(string username)
        {
            return _userRepository.Get(username);
        }

        public void RegisterAdmin(string username, string password)
        {
            var admin = new Admin(username, password);
            AddUser(admin);
        }
    }
}
