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
        private UserManager userManager = null!;
        private AuthManager authManager = null!;

        [TestInitialize]
        public void Setup()
        {
            userManager = new UserManager();
            userManager.Register("melanie", "!123Password");
            authManager = new AuthManager(userManager);
        }

        [TestMethod]
        public void Login_WithValidCredentials_CreatesAndStoresToken()
        {
            string token = authManager.Login("melanie", "!123Password");

            Assert.IsFalse(string.IsNullOrWhiteSpace(token));

            var storedToken = authManager.GetTokenInfo("melanie");

            Assert.IsNotNull(storedToken);
            Assert.AreEqual(token, storedToken.Token);
        }

        [TestMethod]
        public void Login_WithInvalidPassword_ShouldThrowException()
        {
            Assert.ThrowsException<UnauthorizedAccessException>(() => authManager.Login("melanie", "123!Password"));

        }

        [TestMethod]
        public void Login_WithUnknownUser_ShouldThrowException()
        {
            Assert.ThrowsException<InvalidOperationException>(() => authManager.Login("unknown", "something"));
        }

        [TestMethod]
        public void Login_ShouldBeCaseInsensitive()
        {
            string token = authManager.Login("MeLaNiE", "!123Password");

            Assert.AreEqual(token, authManager.GetTokenInfo("melanie")?.Token);
        }

        [TestMethod]
        public void Token_ShouldExpireAfter30Minutes()
        {
            authManager.Login("melanie", "!123Password");
            var tokenInfo = authManager.GetTokenInfo("melanie");

            Assert.IsNotNull(tokenInfo);

            var difference = tokenInfo.ExpiresAt - DateTime.UtcNow;

            Assert.IsTrue(difference.TotalMinutes >= 29.9 && difference.TotalMinutes <= 30.1);
        }

        [TestMethod]
        public void Token_ShouldBeAcceptedOrRejected()
        {
            authManager.Login("melanie", "!123Password");
            var tokenInfo = authManager.GetTokenInfo("melanie");

            Assert.IsNotNull(tokenInfo);

            authManager.ValidateToken("melanie", tokenInfo.Token);

            tokenInfo.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);

            Assert.ThrowsException<UnauthorizedAccessException>(() => authManager.ValidateToken("melanie", tokenInfo.Token));
        }

        [TestMethod]
        public void Token_CanOnlyBeUsedByAssignedUser()
        {
            userManager.Register("lena", "!123Password");
            authManager.Login("melanie", "!123Password");
            authManager.Login("lena", "!123Password");

            var melanieToken = authManager.GetTokenInfo("melanie")?.Token;
            Assert.IsNotNull(melanieToken);

            Assert.ThrowsException<UnauthorizedAccessException>(() =>
            {
                authManager.ValidateToken("lena", melanieToken);
            });
        }

    }
}