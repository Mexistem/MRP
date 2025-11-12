using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRP.Server.Models
{
    public sealed class Admin : User
    {
        public override string Role => "Admin";

        public Admin(string username, string password)
            : base(username, password)
        {
        }
    }
}
