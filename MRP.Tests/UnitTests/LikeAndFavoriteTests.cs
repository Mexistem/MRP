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
    public sealed class LikeAndFavoriteTests
    {
        private InMemoryUserRepository _userRepository = null!;
        private UserManager _userManager = null!;

        private InMemoryMediaRepository _mediaRepository = null!;
        private InMemoryRatingRepository _ratingRepository = null!;

        private InMemoryLikeRepository _likeRepository = null!;
        private InMemoryFavoriteRepository _favoriteRepository = null!;

        private InMemoryTokenRepository _tokenRepository = null!;

        private IMediaManager _mediaManager = null!;
        private ILikeManager _likeManager = null!;
        private IFavoriteManager _favoriteManager = null!;

        [TestInitialize]
        public void Setup()
        {
            var t = TestSetup.Create();

            _userRepository = t.UserRepo;
            _tokenRepository = t.TokenRepo;
            _mediaRepository = t.MediaRepo;
            _ratingRepository = t.RatingRepo;
            _likeRepository = t.LikeRepo;
            _favoriteRepository = t.FavoriteRepo;

            _userManager = new UserManager(_userRepository, _tokenRepository);

            _userManager.Register("melanie", "!123Password");
            _userManager.Register("someone", "!123Password");
            _userManager.Register("third", "!123Password");

            _mediaManager = new MediaManager(_mediaRepository, _userManager);
            _likeManager = new LikeManager(_likeRepository);
            _favoriteManager = new FavoriteManager(_favoriteRepository);
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
                createdBy: createdBy
            );
        }

        private RatingEntry AddRatingDirectly(string mediaTitle, string username, int value = 5, string? comment = null)
        {
            var ratingEntry = new RatingEntry(
                mediaTitle.Trim().ToLowerInvariant(),
                username.Trim(),
                value,
                comment
            );

            _ratingRepository.Add(ratingEntry);
            return ratingEntry;
        }

        [TestMethod]
        public void Like_ForeignRating_IsAllowed_AndIncreasesLikeCount()
        {
            CreateDefaultMedia("melanie", "Inception");
            AddRatingDirectly("Inception", "someone", 5, "great");

            Assert.AreEqual(0, _likeManager.GetLikeCount("Inception", "someone"));

            var likeInfo = _likeManager.LikeRating("Inception", "someone", "melanie");

            Assert.AreEqual("inception", likeInfo.MediaTitle);
            Assert.AreEqual("someone", likeInfo.RatingUsername);
            Assert.AreEqual(1, likeInfo.LikeCount);
            Assert.AreEqual(1, _likeManager.GetLikeCount("Inception", "someone"));
        }

        [TestMethod]
        public void Like_SameUserTwice_ThrowsInvalidOperationException()
        {
            CreateDefaultMedia("melanie", "Inception");
            AddRatingDirectly("Inception", "someone", 5);

            _likeManager.LikeRating("Inception", "someone", "melanie");

            Assert.ThrowsException<InvalidOperationException>(() =>
                _likeManager.LikeRating("Inception", "someone", "melanie")
            );
        }

        [TestMethod]
        public void Unlike_RemovesLike_AndUpdatesLikeCount()
        {
            CreateDefaultMedia("melanie", "Inception");
            AddRatingDirectly("Inception", "someone", 5);

            _likeManager.LikeRating("Inception", "someone", "melanie");
            _likeManager.LikeRating("Inception", "someone", "third");

            Assert.AreEqual(2, _likeManager.GetLikeCount("Inception", "someone"));

            var likeInfoAfterRemove = _likeManager.UnlikeRating("Inception", "someone", "melanie");

            Assert.AreEqual(1, likeInfoAfterRemove.LikeCount);
            Assert.AreEqual(1, _likeManager.GetLikeCount("Inception", "someone"));
        }

        [TestMethod]
        public void LikeCount_ReflectsMultipleUsersCorrectly()
        {
            CreateDefaultMedia("melanie", "Inception");
            AddRatingDirectly("Inception", "someone", 5);

            Assert.AreEqual(0, _likeManager.GetLikeCount("Inception", "someone"));

            _likeManager.LikeRating("Inception", "someone", "melanie");
            Assert.AreEqual(1, _likeManager.GetLikeCount("Inception", "someone"));

            _likeManager.LikeRating("Inception", "someone", "third");
            Assert.AreEqual(2, _likeManager.GetLikeCount("Inception", "someone"));

            _likeManager.UnlikeRating("Inception", "someone", "melanie");
            Assert.AreEqual(1, _likeManager.GetLikeCount("Inception", "someone"));
        }

        [TestMethod]
        public void Favorite_Add_StoresFavorite()
        {
            CreateDefaultMedia("melanie", "Inception");

            _favoriteManager.AddFavorite("melanie", "Inception");

            var favoriteList = _favoriteManager.GetFavorites("melanie");

            Assert.AreEqual("melanie", favoriteList.Username);
            CollectionAssert.AreEqual(
                new List<string> { "inception" },
                favoriteList.MediaTitles.ToList()
            );
        }

        [TestMethod]
        public void Favorite_Remove_RemovesFavorite()
        {
            CreateDefaultMedia("melanie", "Inception");

            _favoriteManager.AddFavorite("melanie", "Inception");
            _favoriteManager.RemoveFavorite("melanie", "Inception");

            var favoriteList = _favoriteManager.GetFavorites("melanie");

            Assert.AreEqual(0, favoriteList.MediaTitles.Count);
        }

        [TestMethod]
        public void Favorite_List_ReturnsAllSavedMedia()
        {
            CreateDefaultMedia("melanie", "Inception");
            CreateDefaultMedia("melanie", "Matrix");
            CreateDefaultMedia("melanie", "Interstellar");

            _favoriteManager.AddFavorite("melanie", "Inception");
            _favoriteManager.AddFavorite("melanie", "Matrix");
            _favoriteManager.AddFavorite("melanie", "Interstellar");

            var favoriteList = _favoriteManager.GetFavorites("melanie");

            CollectionAssert.AreEqual(
                new List<string> { "inception", "matrix", "interstellar" },
                favoriteList.MediaTitles.ToList()
            );
        }
    }
}
