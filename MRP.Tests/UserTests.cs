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
    }
}
