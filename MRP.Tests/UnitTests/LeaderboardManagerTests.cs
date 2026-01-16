using Microsoft.VisualStudio.TestTools.UnitTesting;
using MRP.Server.Models;
using MRP.Server.Services;
using MRP.Tests.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace MRP.Tests
{
    [TestClass]
    public sealed class LeaderboardManagerTests
    {
        private IUserManager _userManager = null!;
        private IMediaManager _mediaManager = null!;
        private IRatingManager _ratingManager = null!;
        private ILikeManager _likeManager = null!;
        private LeaderboardManager _leaderboardManager = null!;

        [TestInitialize]
        public void Setup()
        {
            var t = TestSetup.Create();

            _userManager = new UserManager(t.UserRepo, t.TokenRepo);
            _mediaManager = new MediaManager(t.MediaRepo, _userManager);
            _ratingManager = new RatingManager(t.RatingRepo);
            _likeManager = new LikeManager(t.LikeRepo);

            _leaderboardManager = new LeaderboardManager(
                _userManager,
                _ratingManager,
                _likeManager,
                _mediaManager
            );

            _userManager.Register("alice", "Password1!");
            _userManager.Register("bob", "Password1!");
            _userManager.Register("charlie", "Password1!");

            _mediaManager.CreateMedia(
                "matrix",
                "valid description",
                1999,
                new List<string> { "Sci-Fi" },
                12,
                MediaType.Movie,
                "alice"
            );

            _ratingManager.CreateRating("matrix", "alice", 5, null);
            _ratingManager.CreateRating("matrix", "bob", 3, null);

            _likeManager.LikeRating("matrix", "alice", "bob");
            _likeManager.LikeRating("matrix", "alice", "charlie");
        }

        [TestMethod]
        public void GetLeaderboard_ShouldReturnAllUsers()
        {
            var result = _leaderboardManager.GetLeaderboard().ToList();

            Assert.AreEqual(3, result.Count);
        }

        [TestMethod]
        public void GetLeaderboard_ShouldSortByTotalRatingsThenLikes()
        {
            var result = _leaderboardManager.GetLeaderboard().ToList();

            Assert.AreEqual("alice", result[0].Username);
            Assert.AreEqual(1, result[0].TotalRatings);
            Assert.AreEqual(2, result[0].TotalLikesReceived);
        }

        [TestMethod]
        public void GetLeaderboard_ShouldCalculateAverageScore()
        {
            var alice = _leaderboardManager.GetLeaderboard()
                .First(x => x.Username == "alice");

            Assert.AreEqual(5.0, alice.AverageScore);
        }
    }
}
