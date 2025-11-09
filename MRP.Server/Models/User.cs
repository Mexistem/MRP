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
        public string Username { get; } = null!;
        public string Password { get; private set; } = null!;

        public User(string username, string password) 
        {
            ValidateUsername(username);
            ValidatePassword(password);

            Username = username;
            Password = password;
        }

        private void ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username is not allowed to be empty or contain only whitespace", nameof(username));
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
            {
                throw new ArgumentException("Username cannot contain special characters", nameof(username));
            }

            if (username.Length > 30 || username.Length < 3)
            {
                throw new ArgumentException("Username must be between 3 and 30 characters long", nameof(username));
            }
        }
        private void ValidatePassword(string password)
        {

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password is not allowed to be empty or contain only whitespace", nameof(password));
            }

            if (password.Length < 8 || !System.Text.RegularExpressions.Regex.IsMatch(password, @"[0-9]") || !System.Text.RegularExpressions.Regex.IsMatch(password, @"[!@#$%&*(),.)""':{}|<>]"))
            {
                throw new ArgumentException("Password must be at least 8 characters long and contain at least one number and one special character", nameof(password));
            }
        }

        public override bool Equals(object? obj)
        {
            if (obj is not User otherUser)
            {
                return false;
            }

            return Username == otherUser.Username;
        }
        public override int GetHashCode()
        {
            return Username.GetHashCode();
        }
    }
}
