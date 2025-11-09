using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using MRP.Server.Models;
using MRP.Server.Services;

namespace MRP.Tests.Services
{
    [TestClass]
    public sealed class UserManagerTests
    {
        [TestMethod]
        public void AddUser_ShouldThrowException_WhenUsernameAlreadyExists_CaseInsensitive()
        {
            var manager = new UserManager();
            manager.AddUser(new User("melanie", "!123Password"));

            Assert.ThrowsException<InvalidOperationException>(() => manager.AddUser(new User("mElAnIe", "!123Password")));
        }

    }
}
