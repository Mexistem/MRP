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
            userManager.Register("melanie", "!123Password");

            var authManager = new AuthManager(userManager);

            string token = authManager.Login("melanie", "!123Password");

            Assert.AreEqual("melanie-mrpToken", token);
        }

        [TestMethod]
        public void Login_WithInvalidPassword_ShouldThrowException()
        {
            var userManager = new UserManager();
            userManager.Register("melanie", "!123Password");

            var authManager = new AuthManager(userManager);

            Assert.ThrowsException<UnauthorizedAccessException>(() => authManager.Login("melanie", "123!Password"));

        }

        [TestMethod]
        public void Login_WithUnknownUser_ShouldThrowException()
        {
            var userManager = new UserManager();
            var authManager = new AuthManager(userManager);

            Assert.ThrowsException<InvalidOperationException>(() => authManager.Login("unknown", "something"));
        }

        [TestMethod]
        public void Login_ShouldBeCaseInsensitive()
        {
            var userManager = new UserManager();
            userManager.Register("melanie", "!123Password");

            var authManager = new AuthManager(userManager);

            string token = authManager.Login("MELANIE", "!123Password");

            Assert.AreEqual("melanie-mrpToken", token);
        }
    }
}