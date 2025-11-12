using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MRP.Server.Models
{
    public class User
    {
        private const string UsernamePattern = @"^[a-zA-Z0-9_]+$";
        private const string PasswordSpecialPattern = @"[!@#$%&*(),.)""':{}|<>]";
        public string Username { get; }
        public string Password { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public User(string username, string password)
        {
            ValidateUsername(username);
            ValidatePassword(password, username);

            Username = username;
            Password = HashPassword(username, password);
            CreatedAt = DateTime.UtcNow;
        }

        private static void ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username is not allowed to be empty or contain only whitespace", nameof(username));
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(username, UsernamePattern))
            {
                throw new ArgumentException("Username cannot contain special characters", nameof(username));
            }

            if (username.Length > 30 || username.Length < 3)
            {
                throw new ArgumentException("Username must be between 3 and 30 characters long", nameof(username));
            }
        }
        private static void ValidatePassword(string password, string username)
        {

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password is not allowed to be empty or contain only whitespace", nameof(password));
            }

            if (password.Length < 8 || !System.Text.RegularExpressions.Regex.IsMatch(password, @"[0-9]") || !System.Text.RegularExpressions.Regex.IsMatch(password, PasswordSpecialPattern))
            {
                throw new ArgumentException("Password must be at least 8 characters long and contain at least one number and one special character", nameof(password));
            }

            if (password.ToLower().Contains(username.ToLower()))
            {
                throw new ArgumentException("Password is not allowed to contain the username");
            }
        }

        public static string HashPassword(string username, string password)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(username + password);
            byte[] hash = System.Security.Cryptography.SHA256.HashData(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
