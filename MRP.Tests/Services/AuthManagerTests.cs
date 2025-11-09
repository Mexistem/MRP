using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRP.Server.Models;
using MRP.Server.Services;

namespace MRP.Tests
{
    [TestClass]
    public sealed class AuthManagerTests
    {
        [TestMethod]
        public void Login_WithValidCredentials_ReturnsValidToken()
        {
            var userManager = new UserManager();
            userManager.Register("melanie", "Test123!");

            var authManager = new AuthManager(userManager);

            string token = authManager.Login("melanie", "Test123!");

            Assert.AreEqual("melanie-mrpToken", token);
        }
    }
}