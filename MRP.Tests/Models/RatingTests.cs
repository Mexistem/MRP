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

        [TestMethod]
        public void CreatingRating_ShouldSetCreatedAt()
        {
            var before = DateTime.UtcNow;
            var rating = new RatingEntry("Inception", "melanie", 3, null);
            var after = DateTime.UtcNow;
            Assert.IsTrue(rating.CreatedAt >= before && rating.CreatedAt <= after);
        }

        [TestMethod]
        public void CreatingRating_CommentOptionalAndTrimmed()
        {
            var ratingWithoutComment = new RatingEntry("Inception", "melanie", 3, null);
            Assert.IsNull(ratingWithoutComment.Comment);

            var ratingWithComment = new RatingEntry("Inception", "melanie", 3, "  nice movie  ");
            Assert.AreEqual("nice movie", ratingWithComment.Comment);
        }
    }
}