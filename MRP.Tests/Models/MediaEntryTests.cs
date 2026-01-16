using MRP.Server.Models;
using MRP.Server.Storage.InMemory;
using MRP.Server.Services;
using MRP.Tests.Helpers;

namespace MRP.Tests
{
    [TestClass]
    public class MediaEntryTests
    {
        private InMemoryMediaRepository _repo = null!;
        private MediaManager _manager = null!;
        private InMemoryUserRepository _userRepo = null!;
        private UserManager _userManager = null!;
        private InMemoryRatingRepository _ratingRepo = null!;
        private InMemoryTokenRepository _tokenRepo = null!;
        private InMemoryLikeRepository _likeRepo = null!;
        private InMemoryFavoriteRepository _favoriteRepo = null!;

        [TestInitialize]
        public void Setup()
        {
            var t = TestSetup.Create();

            _userRepo = t.UserRepo;
            _tokenRepo = t.TokenRepo;
            _repo = t.MediaRepo;
            _ratingRepo = t.RatingRepo;
            _likeRepo = t.LikeRepo;
            _favoriteRepo = t.FavoriteRepo;

            _userManager = new UserManager(_userRepo, _tokenRepo);
            _manager = new MediaManager(_repo, _userManager);

            _userManager.Register("melanie", "!123Password");
        }

        [TestMethod]
        public void CreatingMediaEntry_ShouldStoreBasicInformation()
        {
            string title = "Inception";
            string description = "A mind-bending thriller";
            int releaseYear = 2010;
            var genres = new List<string> { "Sci-Fi" };
            int ageRestriction = 12;
            MediaType type = MediaType.Movie;
            string createdBy = "melanie";

            var entry = _manager.CreateMedia(
                title,
                description,
                releaseYear,
                genres,
                ageRestriction,
                type,
                createdBy);

            Assert.AreEqual(title.ToLowerInvariant(), entry.Title);
            Assert.AreEqual(description, entry.Description);
            Assert.AreEqual(releaseYear, entry.ReleaseYear);
            CollectionAssert.AreEqual(genres, entry.Genres);
            Assert.AreEqual(ageRestriction, entry.AgeRestriction);
            Assert.AreEqual(type, entry.Type);
            Assert.AreEqual(createdBy, entry.CreatedBy);
        }

        [TestMethod]
        public void CreatingMediaEntry_WithEmptyTitle_ShouldThrow()
        {
            string title = "";
            string description = "desc";
            int releaseYear = 2020;
            var genres = new List<string> { "Action" };
            int ageRestriction = 12;
            MediaType type = MediaType.Movie;
            string createdBy = "melanie";

            Assert.ThrowsException<ArgumentException>(() =>
            {
                _manager.CreateMedia(
                    title,
                    description,
                    releaseYear,
                    genres,
                    ageRestriction,
                    type,
                    createdBy);
            });
        }

        [TestMethod]
        public void CreatingMediaEntry_ShouldTrimTitle()
        {
            string rawTitle = "   Inception   ";
            string expectedTitle = "inception";
            string description = "A mind-bending thriller";
            int releaseYear = 2010;
            var genres = new List<string> { "Sci-Fi" };
            int ageRestriction = 12;
            MediaType type = MediaType.Movie;
            string creator = "melanie";

            var entry = _manager.CreateMedia(
                rawTitle,
                description,
                releaseYear,
                genres,
                ageRestriction,
                type,
                creator);

            Assert.AreEqual(expectedTitle, entry.Title);
        }

        [TestMethod]
        public void CreatingMediaEntry_WithTitleLengthOutOfRange_ShouldThrow()
        {
            string tooLongTitle = new string('a', 151);
            string description = "A mind-bending thriller";
            int releaseYear = 2010;
            var genres = new List<string> { "Sci-Fi" };
            int ageRestriction = 12;
            MediaType type = MediaType.Movie;
            string creator = "melanie";

            Assert.ThrowsException<ArgumentException>(() =>
            {
                _manager.CreateMedia(
                    tooLongTitle,
                    description,
                    releaseYear,
                    genres,
                    ageRestriction,
                    type,
                    creator);
            });
        }

        [TestMethod]
        public void CreatingMediaEntry_WithEmptyDescription_ShouldThrow()
        {
            string title = "Inception";
            string emptyDescription = "   ";
            int releaseYear = 2010;
            var genres = new List<string> { "Sci-Fi" };
            int ageRestriction = 12;
            MediaType type = MediaType.Movie;
            string creator = "melanie";

            Assert.ThrowsException<ArgumentException>(() =>
            {
                _manager.CreateMedia(
                    title,
                    emptyDescription,
                    releaseYear,
                    genres,
                    ageRestriction,
                    type,
                    creator);
            });
        }

        [TestMethod]
        public void CreatingMediaEntry_ShouldTrimDescription()
        {
            string title = "Inception";
            string rawDescription = "   A mind-bending thriller   ";
            string expectedDescription = "A mind-bending thriller";
            int releaseYear = 2010;
            var genres = new List<string> { "Sci-Fi" };
            int ageRestriction = 12;
            MediaType type = MediaType.Movie;
            string creator = "melanie";

            var entry = _manager.CreateMedia(
                title,
                rawDescription,
                releaseYear,
                genres,
                ageRestriction,
                type,
                creator);

            Assert.AreEqual(expectedDescription, entry.Description);
        }
    }
}
