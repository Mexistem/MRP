using MRP.Server.Models;

namespace MRP.Tests
{
    [TestClass]
    public class RatingEntryTests
    {
        [TestMethod]
        public void CreatingRating_WithValueOutside1To5_ShouldThrow()
        {
            Assert.ThrowsException<ArgumentException>(() =>
            {
                _ = new RatingEntry("Inception", "melanie", 0, null);
            });

            Assert.ThrowsException<ArgumentException>(() =>
            {
                _ = new RatingEntry("Inception", "melanie", 6, null);
            });
        }
    }
}