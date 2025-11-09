using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRP.Server.Models
{
    public class User
    {
        public string Username { get; }

        public User() { }

        public User(string username) 
        {
            throw new ArgumentException();
        }
    }
}
