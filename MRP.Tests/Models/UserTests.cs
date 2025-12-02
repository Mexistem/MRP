using Microsoft.VisualStudio.TestTools.UnitTesting;

using MRP.Server.Models;
using MRP.Server.Services;
using MRP.Server.Storage.InMemory;
using System.Reflection.Metadata;

namespace MRP.Tests.Models
{
    [TestClass]
    public sealed class UserTests
    {
        private static UserManager CreateUserManager()
        {
            var repo = new InMemoryUserRepository();
            return new UserManager(repo);
        }

        [TestMethod]
        public void User_CanBeCreated()
        {
            var user = new User("melanie", "!123Password");

            Assert.IsNotNull(user);
        }

        [TestMethod]
        public void User_ShouldThrowException_WhenUsernameIsEmpty()
        {
            string emptyUsername = "";
            var manager = CreateUserManager();

            Assert.ThrowsException<ArgumentException>(() => manager.Register(emptyUsername, "!123Password"));
        }

        [TestMethod]
        public void User_ShouldThrowException_WhenUsernameIsWhitespace()
        {
            string whitespaceUsername = "   ";
            var manager = CreateUserManager();

            Assert.ThrowsException<ArgumentException>(() => manager.Register(whitespaceUsername, "!123Password"));
        }

        [TestMethod]
        public void User_ShouldStoreUsername_WhenValid()
        {
            string validUsername = "melanie";
            string password = "!123Password";

            var user = new User(validUsername, password);

            Assert.AreEqual(validUsername, user.Username);
        }

        [TestMethod]
        public void User_ShouldThrowException_WhenUsernameContainsSpecialCharacters()
        {
            string invalidUsername = "Mel@n?e!";
            var manager = CreateUserManager();

            Assert.ThrowsException<ArgumentException>( () => manager.Register(invalidUsername, "!123Password"));
        }

        [TestMethod]
        public void User_ShouldThrowException_WhenUsernameIsTooLong()
        {
            string tooLongUsername = new('a', 31);
            var manager = CreateUserManager();

            Assert.ThrowsException<ArgumentException>(() => manager.Register(tooLongUsername, "!123Password"));

        }

        [TestMethod]
        public void User_ShouldThrowException_WhenUsernameIsTooShort()
        {
            string tooShortUsername = "ab";
            var manager = CreateUserManager();

            Assert.ThrowsException<ArgumentException>(() => manager.Register(tooShortUsername, "!123Password"));

        }

        [TestMethod]
        public void User_ShouldThrowException_WhenPasswordIsEmpty()
        {
            string username = "melanie";
            string emptyPassword = "";
            var manager = CreateUserManager();

            Assert.ThrowsException<ArgumentException>(() => manager.Register(username, emptyPassword));
        }

        [TestMethod]

        public void User_ShouldThrowException_WhenPasswordDoesNotMeetComplexityRequirements()
        {
            string username = "melanie";
            string tooShortPassword = "Ab1!";
            string noNumberPassword = "Abcdef@!";
            string noSpecialPassword = "Abcdef12";

            var manager = CreateUserManager();

            Assert.ThrowsException<ArgumentException>(() => manager.Register(username, tooShortPassword));
            Assert.ThrowsException<ArgumentException>(() => manager.Register(username, noNumberPassword));
            Assert.ThrowsException<ArgumentException>(() => manager.Register(username, noSpecialPassword));

        }

        [TestMethod]

        public void User_ShouldThrowException_WhenPasswordContainsUsername()
        {
            string username = "melanie";
            string password = "melanie123!";

            var manager = CreateUserManager();

            Assert.ThrowsException<ArgumentException>(() => manager.Register(username, password));
        }

        [TestMethod]

        public void User_ShouldStorePasswordAsHashedValue()
        {
            string username = "melanie";
            string password = "!123Password";

            var manager = CreateUserManager();

            var user = new User(username, password);

            Assert.AreNotEqual(password, user.Password);
        }

        [TestMethod]
        public void User_ShouldSetCreationDateOnCreation()
        {
            string username = "melanie";
            string password = "!123Password";

            var manager = CreateUserManager();

            var beforeCreation = DateTime.UtcNow;
            var user = new User(username,password);
            var afterCreation = DateTime.UtcNow;

            Assert.IsTrue(user.CreatedAt >= beforeCreation && user.CreatedAt <= afterCreation);
        }


    }
}
