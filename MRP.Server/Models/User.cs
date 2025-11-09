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

        public User(string username) 
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username is not allowed to be empty or contain only whitespace", nameof(username));
            }

            Username = username;
        }
    }
}
