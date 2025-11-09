using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRP.Server.Services
{
    public class AuthManager
    {
        private readonly UserManager _userManager;
        public AuthManager(UserManager userManager)
        {
            _userManager = userManager;
        }

        public string Login(string username, string password)
        {
            var user = _userManager.GetUser(username);
            return $"{username}-mrpToken";
        }
    }
}
