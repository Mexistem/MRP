using Microsoft.VisualStudio.TestTools.UnitTesting;

using MRP.Server.Models;

namespace MRP.Tests.Models
{
    [TestClass]
    public sealed class UserTests
    {
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

            Assert.ThrowsException<ArgumentException>(() => new User(emptyUsername, "!123Password"));
        }

        [TestMethod]
        public void User_ShouldThrowException_WhenUsernameIsWhitespace()
        {
            string whitespaceUsername = "   ";

            Assert.ThrowsException<ArgumentException>(() => new User(whitespaceUsername, "!123Password"));
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
        public void Users_WithSameUsername_ShouldBeEqual()
        {
            var userA = new User("melanie", "!123Password");
            var userB = new User("melanie", "!123Password");

            bool areEqual = userA.Equals(userB);

            Assert.IsTrue(areEqual);

        }

        [TestMethod]
        public void Users_WithDifferentUsernames_ShouldNotBeEqual()
        {
            var userA = new User("melanie", "!123Password");
            var userB = new User("lena", "!123Password");

            bool areEqual = userA.Equals(userB);

            Assert.IsFalse(areEqual);
        }

        [TestMethod]
        public void User_ShouldThrowException_WhenUsernameContainsSpecialCharacters()
        {
            string invalidUsername = "Mel@n?e!";

            Assert.ThrowsException<ArgumentException>( () => new User(invalidUsername, "!123Password"));
        }

        [TestMethod]
        public void User_ShouldThrowException_WhenUsernameIsTooLong()
        {
            string tooLongUsername = new('a', 31);

            Assert.ThrowsException<ArgumentException>(() => new User(tooLongUsername, "!123Password"));

        }

        [TestMethod]
        public void User_ShouldThrowException_WhenUsernameIsTooShort()
        {
            string tooShortUsername = "ab";

            Assert.ThrowsException<ArgumentException>(() => new User(tooShortUsername, "!123Password"));

        }

        [TestMethod]
        public void User_ShouldThrowException_WhenPasswordIsEmpty()
        {
            string username = "melanie";
            string emptyPassword = "";

            Assert.ThrowsException<ArgumentException>(() => new User(username, emptyPassword));
        }

        [TestMethod]

        public void User_ShouldThrowException_WhenPasswordDoesNotMeetComplexityRequirements()
        {
            string username = "melanie";
            string tooShortPassword = "Ab1!";
            string noNumberPassword = "Abcdef!";
            string noSpecialPassword = "Abcdef1";

            Assert.ThrowsException<ArgumentException>(() => new User(username, tooShortPassword));
            Assert.ThrowsException<ArgumentException>(() => new User(username, noNumberPassword));
            Assert.ThrowsException<ArgumentException>(() => new User(username, noSpecialPassword));

        }
    }
}
