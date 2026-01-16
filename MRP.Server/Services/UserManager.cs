using MRP.Server.Models;
using MRP.Server.Validation;
using MRP.Server.Storage.Interfaces;

namespace MRP.Server.Services
{
    public sealed class UserManager : IUserManager
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenRepository _tokenRepository;

        public UserManager(IUserRepository userRepository,
                           ITokenRepository tokenRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _tokenRepository = tokenRepository ?? throw new ArgumentNullException(nameof(tokenRepository));
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
            UserValidator.ValidateUsername(username);
            PasswordValidator.ValidatePassword(password, username);

            var admin = new Admin(username, password);
            AddUser(admin);
        }

        public IEnumerable<User> GetAllUsers()
        {
            return _userRepository.GetAll();
        }

        public void DeleteUser(string username)
        {
            _tokenRepository.RemoveToken(username);
            _userRepository.Delete(username);
        }

        public bool IsAdmin(string username)
        {
            var user = _userRepository.Get(username);
            return user != null && user.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }

        public bool Exists(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }

            return _userRepository.Exists(username.Trim());
        }

        public void DeleteUserAsAdmin(string targetUsername, string requestedBy)
        {
            if (string.IsNullOrWhiteSpace(targetUsername))
            {
                throw new ArgumentException("targetUsername is required", nameof(targetUsername));
            }

            if (string.IsNullOrWhiteSpace(requestedBy))
            {
                throw new ArgumentException("requestedBy is required", nameof(requestedBy));
            }

            targetUsername = targetUsername.Trim();
            requestedBy = requestedBy.Trim();

            if (!IsAdmin(requestedBy))
            {
                throw new UnauthorizedAccessException("Only admins can delete users.");
            }

            if (!_userRepository.Exists(targetUsername))
            {
                throw new KeyNotFoundException("User not found.");
            }

            _tokenRepository.RemoveToken(targetUsername);
            _userRepository.Delete(targetUsername);
        }
    }
}
