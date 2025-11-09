using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRP.Server.Models
{
    public class User
    {
        public string Username { get; } = null!;

        public User() { }

        public User(string username, string password) 
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username is not allowed to be empty or contain only whitespace", nameof(username));
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
            {
                throw new ArgumentException("Username cannot contain special characters", nameof(username));
            }

            if(username.Length > 30 || username.Length < 3)
            {
                throw new ArgumentException("Username must be between 3 and 30 characters long", nameof(username));
            }

            Username = username;
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
