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
    public sealed class AdminDeleteUserTests
    {
        private InMemoryUserRepository _userRepo = null!;
        private InMemoryTokenRepository _tokenRepo = null!;
        private InMemoryMediaRepository _mediaRepo = null!;
        private InMemoryRatingRepository _ratingRepo = null!;
        private InMemoryLikeRepository _likeRepo = null!;
        private InMemoryFavoriteRepository _favoriteRepo = null!;

        private UserManager _userManager = null!;
        private MediaManager _mediaManager = null!;

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
            _mediaManager = new MediaManager(_mediaRepo, _userManager);

            _userManager.RegisterAdmin("admin", "!123Password");
            _userManager.Register("target", "!123Password");
            _userManager.Register("someone", "!123Password");
            _userManager.Register("third", "!123Password");
        }

        private MediaEntry CreateMedia(string title, string createdBy)
        {
            return _mediaManager.CreateMedia(
                title: title,
                description: "desc for testing",
                releaseYear: 2010,
                genres: new List<string> { "Sci-Fi" },
                ageRestriction: 12,
                type: MediaType.Movie,
                createdBy: createdBy);
        }

        private void AddRatingDirect(string mediaTitle, string username, int value = 5, string? comment = null)
        {
            _ratingRepo.Add(new RatingEntry(
                mediaTitle.Trim().ToLowerInvariant(),
                username.Trim(),
                value,
                comment));
        }

        [TestMethod]
        public void DeleteUser_AsNonAdmin_ShouldThrowUnauthorizedAccessException()
        {
            Assert.ThrowsException<UnauthorizedAccessException>(() =>
                _userManager.DeleteUserAsAdmin("target", requestedBy: "someone"));
        }

        [TestMethod]
        public void DeleteUser_AsAdmin_ShouldDeleteUser()
        {
            _userManager.DeleteUserAsAdmin("target", requestedBy: "admin");
            Assert.IsFalse(_userRepo.Exists("target"));
        }

        [TestMethod]
        public void DeleteUser_ShouldRemoveUserTokens()
        {
            _tokenRepo.SetToken("target", new TokenInfo
            {
                Token = "token123",
                ExpiresAt = DateTime.UtcNow.AddMinutes(30)
            });

            _userManager.DeleteUserAsAdmin("target", requestedBy: "admin");

            Assert.IsNull(_tokenRepo.GetByUsername("target"));
        }

        [TestMethod]
        public void DeleteUser_ShouldRemoveUserRatings()
        {
            CreateMedia("inception", "someone");
            AddRatingDirect("inception", "target", 5, "nice");
            AddRatingDirect("inception", "someone", 4, "ok");

            _userManager.DeleteUserAsAdmin("target", requestedBy: "admin");

            Assert.IsNull(_ratingRepo.GetByMediaTitleAndUsername("inception", "target"));
            Assert.IsNotNull(_ratingRepo.GetByMediaTitleAndUsername("inception", "someone"));
        }

        [TestMethod]
        public void DeleteUser_ShouldRemoveUserLikes()
        {
            CreateMedia("inception", "someone");
            AddRatingDirect("inception", "someone", 5);

            _likeRepo.Add("inception", "someone", "target");

            _userManager.DeleteUserAsAdmin("target", requestedBy: "admin");

            Assert.IsFalse(_likeRepo.Exists("inception", "someone", "target"));
        }

        [TestMethod]
        public void DeleteUser_ShouldRemoveUserFavorites()
        {
            CreateMedia("inception", "someone");

            _favoriteRepo.Add("target", "inception");
            Assert.IsTrue(_favoriteRepo.Exists("target", "inception"));

            _userManager.DeleteUserAsAdmin("target", requestedBy: "admin");
            Assert.IsFalse(_favoriteRepo.Exists("target", "inception"));
        }

        [TestMethod]
        public void DeleteUser_ShouldDeleteUserMedia_IncludingRelatedRatingsLikesFavorites()
        {
            CreateMedia("matrix", "target");
            AddRatingDirect("matrix", "someone", 5);
            _likeRepo.Add("matrix", "someone", "third");
            _favoriteRepo.Add("someone", "matrix");

            Assert.IsNotNull(_mediaRepo.GetByTitle("matrix"));
            Assert.IsNotNull(_ratingRepo.GetByMediaTitleAndUsername("matrix", "someone"));
            Assert.AreEqual(1, _likeRepo.CountForRating("matrix", "someone"));
            Assert.IsTrue(_favoriteRepo.Exists("someone", "matrix"));

            _userManager.DeleteUserAsAdmin("target", requestedBy: "admin");

            Assert.IsNull(_mediaRepo.GetByTitle("matrix"));
            Assert.IsNull(_ratingRepo.GetByMediaTitleAndUsername("matrix", "someone"));
            Assert.AreEqual(0, _likeRepo.CountForRating("matrix", "someone"));
            Assert.IsFalse(_favoriteRepo.Exists("someone", "matrix"));
        }
    }
}
