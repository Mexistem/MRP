using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRP.Server.Models;

namespace MRP.Tests
{
    [TestClass]
    public sealed class UserTests
    {
        [TestMethod]
        public void User_CanBeCreated()
        {
            var user = new User();

            Assert.IsNotNull(user);
        }

        [TestMethod]
        public void User_ShouldThrowException_WhenUsernameIsEmpty()
        {
            string emptyUsername = "";

            Assert.ThrowsException<ArgumentException>(() => new User(emptyUsername));
        }

        [TestMethod]
        public void User_ShouldThrowException_WhenUsernameIsWhitespace()
        {
            string whitespaceUsername = "   ";

            Assert.ThrowsException<ArgumentException>(() => new User(whitespaceUsername));
        }

        [TestMethod]
        public void User_ShouldStoreUsername_WhenValid()
        {
            string validUsername = "melanie";

            var user = new User(validUsername);

            Assert.AreEqual(validUsername, user.Username);
        }

        [TestMethod]
        public void Users_WithSameUsername_ShouldBeEqual()
        {
            var userA = new User("melanie");
            var userB = new User("melanie");

            bool areEqual = userA.Equals(userB);

            Assert.IsTrue(areEqual);

        }

        [TestMethod]
        public void Users_WithDifferentUsernames_ShouldNotBeEqual()
        {
            var userA = new User("melanie");
            var userB = new User("lena");

            bool areEqual = userA.Equals(userB);

            Assert.IsFalse(areEqual);
        }

        [TestMethod]
        public void User_ShouldThrowException_WhenUsernameContainsSpecialCharacters()
        {
            string invalidUsername = "Mel@n?e!";

            Assert.ThrowsException<ArgumentException>( () => new User(invalidUsername));
        }

        [TestMethod]
        public void User_ShouldThrowException_WhenUsernameIsTooLong()
        {
            string tooLongUsername = new string('a', 31);

            Assert.ThrowsException<ArgumentException>(() => new User(tooLongUsername));

        }
    }
}
