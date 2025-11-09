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

            Assert.ThrowsException<ArgumentException>(() =>
            {
                new User(emptyUsername);
            });
        }
    }
}
