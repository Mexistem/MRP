using System.Data;

namespace MRP.Server.Models
{
    public class User
    {
        public string Username { get; }
        public string Password { get; protected set; }
        public string Role { get; protected set; }
        public DateTime CreatedAt { get; protected set; }

        public User(string username, string password)
        {
            Username = username;
            Password = HashPassword(username, password);
            CreatedAt = DateTime.UtcNow;
            Role = "User";
        }


        protected User(string username, string password, string role)
        {
            Username = username;
            Password = HashPassword(username, password);
            CreatedAt = DateTime.UtcNow;
            Role = role;
        }

        internal User(string username, string passwordHash, string role, DateTime createdAt)
        {
            Username = username;
            Password = passwordHash;   
            CreatedAt = createdAt;
            Role = role;
        }

        public static string HashPassword(string username, string password)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(username.ToLower() + password);
            byte[] hash = System.Security.Cryptography.SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
