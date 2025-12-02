using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRP.Server.Models;
using MRP.Server.Services;
using MRP.Server.Storage.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRP.Tests.Services
{
    [TestClass]
    public sealed class UserManagerTests
    {
        [TestMethod]
        public void AddUser_ShouldThrowException_WhenUsernameAlreadyExists_CaseInsensitive()
        {
            var userRepository = new InMemoryUserRepository();
            var manager = new UserManager(userRepository);

            manager.Register("melanie", "!123Password");

            Assert.ThrowsException<InvalidOperationException>(() => manager.Register("mElAnIe", "!123Password"));
        }

    }
}
