using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRP.Server.Services;
using MRP.Tests.Helpers;
using System;

namespace MRP.Tests.Services
{
    [TestClass]
    public sealed class UserManagerTests
    {
        [TestMethod]
        public void AddUser_ShouldThrowException_WhenUsernameAlreadyExists_CaseInsensitive()
        {
            var t = TestSetup.Create();

            var manager = new UserManager(
                t.UserRepo,
                t.TokenRepo
            );

            manager.Register("melanie", "!123Password");

            Assert.ThrowsException<InvalidOperationException>(() => manager.Register("mElAnIe", "!123Password"));
        }
    }
}
