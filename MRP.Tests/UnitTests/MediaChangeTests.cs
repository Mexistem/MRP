using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRP.Server.Models;
using MRP.Server.Services;
using MRP.Server.Storage.InMemory;
using MRP.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MRP.Tests.Services
{
    [TestClass]
    public sealed class MediaChangeTests
    {
        private InMemoryUserRepository _userRepo = null!;
        private UserManager _userManager = null!;

        private InMemoryMediaRepository _mediaRepo = null!;
        private InMemoryRatingRepository _ratingRepo = null!;

        private InMemoryTokenRepository _tokenRepo = null!;
        private InMemoryLikeRepository _likeRepo = null!;
        private InMemoryFavoriteRepository _favoriteRepo = null!;

        private IMediaManager _mediaManager = null!;

        [TestInitialize]
        public void Setup()
        {
            var t = TestSetup.Create();

            _userRepo = t.UserRepo;
            _tokenRepo = t.TokenRepo;
            _mediaRepo = t.MediaRepo;
            _ratingRepo = t.RatingRepo;
            _likeRepo = t.LikeRepo;
            _favoriteRepo = t.FavoriteRepo;

            _userManager = new UserManager(_userRepo, _tokenRepo);

            _userManager.Register("melanie", "!123Password");
            _userManager.Register("someone", "!123Password");
            _userManager.RegisterAdmin("admin", "!123Password");

            _mediaManager = new MediaManager(_mediaRepo, _userManager);
        }

        private MediaEntry CreateDefaultMedia(string createdBy, string title = "Inception")
        {
            return _mediaManager.CreateMedia(
                title: title,
                description: "A mind-bending thriller",
                releaseYear: 2010,
                genres: new List<string> { "Sci-Fi" },
                ageRestriction: 12,
                type: MediaType.Movie,
                createdBy: createdBy);
        }

        private void AddRating(string mediaTitle, string username, int value = 5, string? comment = null)
        {
            var rating = new RatingEntry(mediaTitle.Trim().ToLowerInvariant(), username.Trim(), value, comment);
            _ratingRepo.Add(rating);
        }

        [TestMethod]
        public void UpdateMedia_AsCreator_ShouldUpdateFields_KeepCreatedAt_AndChangeLastModifiedAt()
        {
            var created = CreateDefaultMedia("melanie", "Inception");
            var createdAt = created.CreatedAt;
            var oldLastModified = created.LastModifiedAt;

            var before = DateTime.UtcNow;

            var updated = _mediaManager.UpdateMedia(
                title: "Inception",
                newTitle: null,
                description: "  Updated description  ",
                releaseYear: 2000,
                genres: new List<string> { " Sci-Fi ", "Thriller" },
                ageRestriction: 16,
                type: MediaType.Movie,
                requestedBy: "melanie"
            );

            var after = DateTime.UtcNow;

            Assert.AreEqual("inception", updated.Title);
            Assert.AreEqual("Updated description", updated.Description);
            Assert.AreEqual(2000, updated.ReleaseYear);
            CollectionAssert.AreEqual(new List<string> { "Sci-Fi", "Thriller" }, updated.Genres);
            Assert.AreEqual(16, updated.AgeRestriction);
            Assert.AreEqual(MediaType.Movie, updated.Type);

            Assert.AreEqual(createdAt, updated.CreatedAt);
            Assert.AreNotEqual(oldLastModified, updated.LastModifiedAt);
            Assert.IsTrue(updated.LastModifiedAt >= before);
            Assert.IsTrue(updated.LastModifiedAt <= after);
        }

        [TestMethod]
        public void UpdateMedia_AsNonCreatorNonAdmin_ShouldThrowUnauthorized()
        {
            CreateDefaultMedia("melanie", "Inception");

            Assert.ThrowsException<UnauthorizedAccessException>(() =>
                _mediaManager.UpdateMedia(
                    title: "Inception",
                    newTitle: null,
                    description: "Updated",
                    releaseYear: 2011,
                    genres: new List<string> { "Sci-Fi" },
                    ageRestriction: 12,
                    type: MediaType.Movie,
                    requestedBy: "someone"
                ));
        }

        [TestMethod]
        public void UpdateMedia_AsAdmin_ShouldBeAllowed()
        {
            CreateDefaultMedia("melanie", "Inception");

            var updated = _mediaManager.UpdateMedia(
                title: "Inception",
                newTitle: null,
                description: "Admin update",
                releaseYear: 2011,
                genres: new List<string> { "Sci-Fi" },
                ageRestriction: 12,
                type: MediaType.Movie,
                requestedBy: "admin"
            );

            Assert.AreEqual("inception", updated.Title);
            Assert.AreEqual("Admin update", updated.Description);
        }

        [TestMethod]
        public void UpdateMedia_WhenMediaDoesNotExist_ShouldThrowKeyNotFound()
        {
            Assert.ThrowsException<KeyNotFoundException>(() =>
                _mediaManager.UpdateMedia(
                    title: "does-not-exist",
                    newTitle: null,
                    description: "Updated",
                    releaseYear: 2011,
                    genres: new List<string> { "Sci-Fi" },
                    ageRestriction: 12,
                    type: MediaType.Movie,
                    requestedBy: "melanie"
                ));
        }

        [TestMethod]
        public void UpdateMedia_ShouldRejectEmptyTitle()
        {
            CreateDefaultMedia("melanie", "Inception");

            Assert.ThrowsException<ArgumentException>(() =>
                _mediaManager.UpdateMedia(
                    title: "   ",
                    newTitle: null,
                    description: "Updated",
                    releaseYear: 2011,
                    genres: new List<string> { "Sci-Fi" },
                    ageRestriction: 12,
                    type: MediaType.Movie,
                    requestedBy: "melanie"
                ));
        }

        [TestMethod]
        public void UpdateMedia_ShouldRejectEmptyDescription()
        {
            CreateDefaultMedia("melanie", "Inception");

            Assert.ThrowsException<ArgumentException>(() =>
                _mediaManager.UpdateMedia(
                    title: "Inception",
                    newTitle: null,
                    description: "   ",
                    releaseYear: 2011,
                    genres: new List<string> { "Sci-Fi" },
                    ageRestriction: 12,
                    type: MediaType.Movie,
                    requestedBy: "melanie"
                ));
        }

        [TestMethod]
        public void UpdateMedia_ShouldRejectEmptyGenres()
        {
            CreateDefaultMedia("melanie", "Inception");

            Assert.ThrowsException<ArgumentException>(() =>
                _mediaManager.UpdateMedia(
                    title: "Inception",
                    newTitle: null,
                    description: "Updated",
                    releaseYear: 2011,
                    genres: new List<string>(),
                    ageRestriction: 12,
                    type: MediaType.Movie,
                    requestedBy: "melanie"
                ));
        }

        [TestMethod]
        public void UpdateMedia_ShouldRejectInvalidAgeRestriction()
        {
            CreateDefaultMedia("melanie", "Inception");

            Assert.ThrowsException<ArgumentException>(() =>
                _mediaManager.UpdateMedia(
                    title: "Inception",
                    newTitle: null,
                    description: "Updated",
                    releaseYear: 2011,
                    genres: new List<string> { "Sci-Fi" },
                    ageRestriction: -1,
                    type: MediaType.Movie,
                    requestedBy: "melanie"
                ));
        }

        [TestMethod]
        public void UpdateMedia_ShouldRejectInvalidReleaseYear()
        {
            CreateDefaultMedia("melanie", "Inception");

            Assert.ThrowsException<ArgumentException>(() =>
                _mediaManager.UpdateMedia(
                    title: "Inception",
                    newTitle: null,
                    description: "Updated",
                    releaseYear: 0,
                    genres: new List<string> { "Sci-Fi" },
                    ageRestriction: 12,
                    type: MediaType.Movie,
                    requestedBy: "melanie"
                ));
        }

        [TestMethod]
        public void UpdateMedia_ShouldRejectInvalidType()
        {
            CreateDefaultMedia("melanie", "Inception");

            Assert.ThrowsException<ArgumentException>(() =>
                _mediaManager.UpdateMedia(
                    title: "Inception",
                    newTitle: null,
                    description: "Updated",
                    releaseYear: 2011,
                    genres: new List<string> { "Sci-Fi" },
                    ageRestriction: 12,
                    type: (MediaType)999,
                    requestedBy: "melanie"
                ));
        }

        [TestMethod]
        public void UpdateMedia_WithNewTitle_ShouldRename_AndPreserveCreatedAt_AndUpdateLastModifiedAt()
        {
            var created = CreateDefaultMedia("melanie", "Inception");
            var createdAt = created.CreatedAt;

            var before = DateTime.UtcNow;

            var updated = _mediaManager.UpdateMedia(
                title: "Inception",
                newTitle: "  Matrix  ",
                description: "Updated description",
                releaseYear: 2010,
                genres: new List<string> { "Sci-Fi" },
                ageRestriction: 12,
                type: MediaType.Movie,
                requestedBy: "melanie"
            );

            var after = DateTime.UtcNow;

            Assert.AreEqual("matrix", updated.Title);
            Assert.AreEqual(createdAt, updated.CreatedAt);
            Assert.IsTrue(updated.LastModifiedAt >= before);
            Assert.IsTrue(updated.LastModifiedAt <= after);

            Assert.IsNull(_mediaManager.GetByTitle("Inception"));
            Assert.IsNotNull(_mediaManager.GetByTitle("Matrix"));
        }

        [TestMethod]
        public void UpdateMedia_WithWhitespaceNewTitle_ShouldThrowArgumentException()
        {
            CreateDefaultMedia("melanie", "Inception");

            Assert.ThrowsException<ArgumentException>(() =>
                _mediaManager.UpdateMedia(
                    title: "Inception",
                    newTitle: "   ",
                    description: "Updated",
                    releaseYear: 2011,
                    genres: new List<string> { "Sci-Fi" },
                    ageRestriction: 12,
                    type: MediaType.Movie,
                    requestedBy: "melanie"
                ));
        }

        [TestMethod]
        public void UpdateMedia_RenameToDuplicateTitle_ShouldThrowInvalidOperation()
        {
            CreateDefaultMedia("melanie", "Inception");
            CreateDefaultMedia("melanie", "Matrix");

            Assert.ThrowsException<InvalidOperationException>(() =>
                _mediaManager.UpdateMedia(
                    title: "Inception",
                    newTitle: "Matrix",
                    description: "Updated",
                    releaseYear: 2011,
                    genres: new List<string> { "Sci-Fi" },
                    ageRestriction: 12,
                    type: MediaType.Movie,
                    requestedBy: "melanie"
                ));
        }

        [TestMethod]
        public void DeleteMedia_AsNonCreatorNonAdmin_ShouldThrowUnauthorized()
        {
            CreateDefaultMedia("melanie", "Inception");

            Assert.ThrowsException<UnauthorizedAccessException>(() =>
                _mediaManager.DeleteMedia("Inception", "someone"));
        }

        [TestMethod]
        public void DeleteMedia_AsAdmin_ShouldBeAllowed()
        {
            CreateDefaultMedia("melanie", "Inception");

            _mediaManager.DeleteMedia("Inception", "admin");

            Assert.IsNull(_mediaManager.GetByTitle("Inception"));
        }

        [TestMethod]
        public void DeleteMedia_ShouldRemoveRelatedRatings()
        {
            var inception = CreateDefaultMedia("melanie", "Inception");
            AddRating(inception.Title, "melanie", 5);
            AddRating(inception.Title, "someone", 4);

            var matrix = CreateDefaultMedia("melanie", "Matrix");
            AddRating(matrix.Title, "melanie", 5);

            _mediaManager.DeleteMedia("Inception", "melanie");

            var remaining = _ratingRepo.GetAll().ToList();
            Assert.AreEqual(1, remaining.Count);
            Assert.AreEqual("matrix", remaining[0].MediaTitle.Trim().ToLowerInvariant());
        }

        [TestMethod]
        public void DeleteMedia_WhenMediaDoesNotExist_ShouldThrowKeyNotFound()
        {
            Assert.ThrowsException<KeyNotFoundException>(() =>
                _mediaManager.DeleteMedia("does-not-exist", "melanie"));
        }

        [TestMethod]
        public void DeleteMedia_ShouldBeCaseInsensitive()
        {
            CreateDefaultMedia("melanie", "Inception");

            _mediaManager.DeleteMedia("INCEPTION", "melanie");

            Assert.IsNull(_mediaManager.GetByTitle("inception"));
        }

        [TestMethod]
        public void UpdateMedia_Rename_ShouldMoveRatings()
        {
            var inception = CreateDefaultMedia("melanie", "Inception");
            AddRating(inception.Title, "melanie", 5);
            AddRating(inception.Title, "someone", 4);

            _mediaManager.UpdateMedia(
                title: "Inception",
                newTitle: "Matrix",
                description: "Updated description",
                releaseYear: 2010,
                genres: new List<string> { "Sci-Fi" },
                ageRestriction: 12,
                type: MediaType.Movie,
                requestedBy: "melanie"
            );

            var ratings = _ratingRepo.GetAll().ToList();
            Assert.AreEqual(2, ratings.Count);
            Assert.IsTrue(ratings.All(r => r.MediaTitle.Trim().ToLowerInvariant() == "matrix"));
        }
    }
}
