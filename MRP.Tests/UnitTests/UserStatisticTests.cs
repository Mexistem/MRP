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
    public sealed class UserStatisticTests
    {
        private InMemoryUserRepository _userRepo = null!;
        private UserManager _userManager = null!;

        private InMemoryMediaRepository _mediaRepo = null!;
        private InMemoryRatingRepository _ratingRepo = null!;
        private InMemoryFavoriteRepository _favoriteRepo = null!;

        private IMediaManager _mediaManager = null!;
        private IRatingManager _ratingManager = null!;
        private IFavoriteManager _favoriteManager = null!;

        private InMemoryTokenRepository _tokenRepo = null!;
        private InMemoryLikeRepository _likeRepo = null!;

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

            _mediaManager = new MediaManager(_mediaRepo , _userManager);
            _ratingManager = new RatingManager(_ratingRepo);
            _favoriteManager = new FavoriteManager(_favoriteRepo);
        }

        private MediaEntry CreateMedia(string title, List<string> genres)
        {
            return _mediaManager.CreateMedia(
                title: title,
                description: "A valid description",
                releaseYear: 2000,
                genres: genres,
                ageRestriction: 12,
                type: MediaType.Movie,
                createdBy: "melanie"
            );
        }

        private RatingEntry AddRatingWithCreatedAt(string mediaTitle, string username, int value, DateTime createdAtUtc)
        {
            var r = RatingEntry.FromDatabase(
                mediaTitle: mediaTitle.Trim().ToLowerInvariant(),
                username: username.Trim(),
                value: value,
                comment: null,
                createdAt: createdAtUtc
            );

            _ratingRepo.Add(r);
            return r;
        }

        private RatingEntry AddRatingUsingDefaultCtorUtcNow(string mediaTitle, string username, int value, string? comment = null)
        {
            var r = new RatingEntry(
                mediaTitle.Trim().ToLowerInvariant(),
                username.Trim(),
                value,
                comment
            );

            _ratingRepo.Add(r);
            return r;
        }

        private static UserStatistics ComputeStats(
            string username,
            IRatingManager ratingManager,
            IMediaManager mediaManager,
            IFavoriteManager favoriteManager)
        {
            var allUserRatings = ratingManager.GetAllRatings()
                .Where(r => string.Equals(r.Username, username, StringComparison.Ordinal))
                .ToList();

            var ratings = allUserRatings
                .Where(r => mediaManager.GetByTitle(r.MediaTitle) is not null)
                .ToList();

            int totalRatings = ratings.Count;

            double averageScore = 0;
            int? highestScore = null;
            int? lowestScore = null;
            DateTime? lastRatedAt = null;
            int ratedMediaCount = 0;
            string? favoriteGenre = null;

            if (totalRatings > 0)
            {
                averageScore = ratings.Average(r => r.Value);
                highestScore = ratings.Max(r => r.Value);
                lowestScore = ratings.Min(r => r.Value);
                lastRatedAt = ratings.Max(r => r.CreatedAt);

                ratedMediaCount = ratings
                    .Select(r => r.MediaTitle)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count();

                var topGenre = ratings
                    .Select(r => mediaManager.GetByTitle(r.MediaTitle))
                    .Where(m => m is not null)
                    .SelectMany(m => m!.Genres ?? Enumerable.Empty<string>())
                    .Where(g => !string.IsNullOrWhiteSpace(g))
                    .Select(g => g.Trim())
                    .GroupBy(g => g, StringComparer.OrdinalIgnoreCase)
                    .Select(grp => new { Genre = grp.Key, Count = grp.Count() })
                    .OrderByDescending(x => x.Count)
                    .ThenBy(x => x.Genre, StringComparer.OrdinalIgnoreCase)
                    .FirstOrDefault();

                favoriteGenre = topGenre?.Genre;
            }

            var favorites = favoriteManager.GetFavorites(username);

            int totalFavorites = (favorites.MediaTitles ?? new List<string>())
                .Count(t => mediaManager.GetByTitle(t) is not null);

            return new UserStatistics
            {
                TotalRatings = totalRatings,
                AverageScore = averageScore,
                FavoriteGenre = favoriteGenre,
                RatedMediaCount = ratedMediaCount,
                HighestScore = highestScore,
                LowestScore = lowestScore,
                TotalFavorites = totalFavorites,
                LastRatedAt = lastRatedAt
            };
        }

        [TestMethod]
        public void ComputeStats_WhenNoRatingsAndNoFavorites_ShouldReturnZeros()
        {
            var stats = ComputeStats("melanie", _ratingManager, _mediaManager, _favoriteManager);

            Assert.AreEqual(0, stats.TotalRatings);
            Assert.AreEqual(0.0, stats.AverageScore);
            Assert.IsNull(stats.FavoriteGenre);
            Assert.AreEqual(0, stats.RatedMediaCount);
            Assert.IsNull(stats.HighestScore);
            Assert.IsNull(stats.LowestScore);
            Assert.AreEqual(0, stats.TotalFavorites);
            Assert.IsNull(stats.LastRatedAt);
        }

        [TestMethod]
        public void ComputeStats_WithRatings_ShouldCalculateAverageMinMaxAndLastRatedAt()
        {
            CreateMedia("inception", new List<string> { "Sci-Fi" });

            var t1 = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            var t2 = new DateTime(2025, 1, 2, 10, 0, 0, DateTimeKind.Utc);

            AddRatingWithCreatedAt("inception", "melanie", 5, t1);
            AddRatingWithCreatedAt("inception", "someone", 1, t2);

            var stats = ComputeStats("melanie", _ratingManager, _mediaManager, _favoriteManager);

            Assert.AreEqual(1, stats.TotalRatings);
            Assert.AreEqual(5.0, stats.AverageScore);
            Assert.AreEqual(5, stats.HighestScore);
            Assert.AreEqual(5, stats.LowestScore);
            Assert.AreEqual(t1, stats.LastRatedAt);
            Assert.AreEqual(1, stats.RatedMediaCount);
        }


        [TestMethod]
        public void ComputeStats_FavoriteGenre_ShouldReturnMostCommonGenreAcrossRatedMedia()
        {
            CreateMedia("inception", new List<string> { "Sci-Fi", "Thriller" });
            CreateMedia("matrix", new List<string> { "Sci-Fi", "Action" });

            AddRatingUsingDefaultCtorUtcNow("inception", "melanie", 5);
            AddRatingUsingDefaultCtorUtcNow("matrix", "melanie", 4);

            var stats = ComputeStats("melanie", _ratingManager, _mediaManager, _favoriteManager);

            Assert.AreEqual("Sci-Fi", stats.FavoriteGenre);
        }
    }
}
