using MRP.Server.Models;
using MRP.Server.Services;
using MRP.Tests.Helpers;

namespace MRP.Tests
{
    [TestClass]
    public class RatingEntryTests
    {
        private RatingManager CreateManager()
        {
            var t = TestSetup.Create();

            var userManager = new UserManager(t.UserRepo, t.TokenRepo);
            var mediaManager = new MediaManager(t.MediaRepo, userManager);

            userManager.Register("melanie", "password123!");
            userManager.RegisterAdmin("admin", "password123!");

            mediaManager.CreateMedia(
                "Matrix",
                "Test Description",
                1999,
                new List<string> { "Sci-Fi", "Action" },
                16,
                MediaType.Movie,
                "admin"
            );

            return new RatingManager(t.RatingRepo);
        }

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

        [TestMethod]
        public void CreateRating_WhenRatingAlreadyExists_ShouldThrow()
        {
            var manager = CreateManager();

            manager.CreateRating("Matrix", "melanie", 5, "Top");

            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                manager.CreateRating("Matrix", "melanie", 4, "Changed");
            });
        }

        [TestMethod]
        public void UpdateRating_ShouldUpdateValueAndComment_AndKeepCreatedAt()
        {
            var manager = CreateManager();

            var created = manager.CreateRating("Matrix", "melanie", 5, "Top");
            var createdAt = created.CreatedAt;

            var updated = manager.UpdateRating("Matrix", "melanie", 2, "  meh  ");

            Assert.AreEqual(2, updated.Value);
            Assert.AreEqual("meh", updated.Comment);
            Assert.AreEqual(createdAt, updated.CreatedAt);
        }
    }
}
