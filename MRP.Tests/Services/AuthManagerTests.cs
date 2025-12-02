using MRP.Server.Services;
using MRP.Server.Storage.InMemory;

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
            var userRepository = new InMemoryUserRepository();
            userManager = new UserManager(userRepository);
            userManager.Register("melanie", "!123Password");
            var tokenRepository = new InMemoryTokenRepository();
            authManager = new AuthManager(userManager, tokenRepository);
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

            Assert.ThrowsException<UnauthorizedAccessException>(() => authManager.ValidateToken("lena", melanieToken));
        }

        [TestMethod]
        public void LoggedOutUser_LosesTokenValidity()
        {
            authManager.Login("melanie", "!123Password");
            var tokenInfo = authManager.GetTokenInfo("melanie");

            Assert.IsNotNull(tokenInfo);

            authManager.Logout("melanie");

            Assert.ThrowsException<UnauthorizedAccessException>(() => authManager.ValidateToken("melanie", tokenInfo.Token));
        }

        [TestMethod]
        public void ExpiredToken_ShouldBeRemovedFromStore_WhenValidated()
        {
            authManager.Login("melanie", "!123Password");

            var tokenInfo = authManager.GetTokenInfo("melanie");

            Assert.IsNotNull(tokenInfo);

            tokenInfo!.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);

            Assert.ThrowsException<UnauthorizedAccessException>(() => authManager.ValidateToken("melanie", tokenInfo.Token));

            var afterValidation = authManager.GetTokenInfo("melanie");
            Assert.IsNull(afterValidation, "Expired token should be removed from the store after validation");

        }
    }
}