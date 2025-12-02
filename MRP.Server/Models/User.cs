namespace MRP.Server.Models
{
    public class User
    {
        public string Username { get; }
        public string Password { get; private set; }
        public virtual string Role => "User";
        public DateTime CreatedAt { get; private set; }

        public User(string username, string password)
        {
            Username = username;
            Password = HashPassword(username, password);
            CreatedAt = DateTime.UtcNow;
        }

        public static string HashPassword(string username, string password)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(username.ToLower() + password);
            byte[] hash = System.Security.Cryptography.SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
