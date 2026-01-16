using MRP.Server.Models;
using MRP.Server.Services.Interfaces;
using MRP.Server.Storage.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MRP.Server.Services
{
    public sealed class UserStatisticsManager : IUserStatisticsManager
    {
        private readonly IRatingManager _ratingManager;
        private readonly IMediaManager _mediaManager;
        private readonly IFavoriteManager _favoriteManager;
        private readonly ILikeManager _likeManager;

        public UserStatisticsManager(
            IRatingManager ratingManager,
            IMediaManager mediaManager,
            IFavoriteManager favoriteManager,
            ILikeManager likeManager)
        {
            _ratingManager = ratingManager;
            _mediaManager = mediaManager;
            _favoriteManager = favoriteManager;
            _likeManager = likeManager;

        }

        public UserStatistics ComputePublic(string username)
        {
            var core = ComputeCore(username);

            return new UserStatistics
            {
                TotalRatings = core.TotalRatings,
                TotalLikesReceived = core.TotalLikesReceived,
                AverageScore = core.AverageScore,
                FavoriteGenre = core.FavoriteGenre,
                RatedMediaCount = core.RatedMediaCount,
                HighestScore = core.HighestScore,
                LowestScore = core.LowestScore,
                TotalFavorites = 0,
                LastRatedAt = null
            };
        }

        public UserStatistics ComputePrivate(string username)
        {
            var core = ComputeCore(username);

            var favorites = _favoriteManager.GetFavorites(username);
            var favoriteTitles = favorites.MediaTitles ?? new List<string>();

            int totalFavorites = favoriteTitles
                .Count(t => _mediaManager.GetByTitle(t) is not null);

            return new UserStatistics
            {
                TotalRatings = core.TotalRatings,
                TotalLikesReceived = core.TotalLikesReceived,
                AverageScore = core.AverageScore,
                FavoriteGenre = core.FavoriteGenre,
                RatedMediaCount = core.RatedMediaCount,
                HighestScore = core.HighestScore,
                LowestScore = core.LowestScore,
                TotalFavorites = totalFavorites,
                LastRatedAt = core.LastRatedAt
            };
        }

        private Core ComputeCore(string username)
        {
            var ratings = _ratingManager.GetAllRatings()
                .Where(r => r.Username == username)
                .ToList();

            if (ratings.Count == 0)
            {
                return new Core(
                    0,
                    0,
                    0,
                    null,
                    0,
                    null,
                    null,
                    null
                );
            }

            int totalLikesReceived = ratings.Sum(r =>
                _likeManager.GetLikeCount(r.MediaTitle, r.Username));

            double avg = ratings.Average(r => r.Value);
            int highest = ratings.Max(r => r.Value);
            int lowest = ratings.Min(r => r.Value);
            DateTime lastRatedAt = ratings.Max(r => r.CreatedAt);

            int ratedMediaCount = ratings
                .Select(r => r.MediaTitle)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            string? favoriteGenre = ratings
                .Select(r => _mediaManager.GetByTitle(r.MediaTitle))
                .Where(m => m != null)
                .SelectMany(m => m!.Genres ?? Enumerable.Empty<string>())
                .Where(g => !string.IsNullOrWhiteSpace(g))
                .Select(g => g.Trim())
                .GroupBy(g => g, StringComparer.OrdinalIgnoreCase)
                .Select(g => new { Genre = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ThenBy(x => x.Genre, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault()
                ?.Genre;

            return new Core(
                ratings.Count,
                totalLikesReceived,
                avg,
                favoriteGenre,
                ratedMediaCount,
                highest,
                lowest,
                lastRatedAt
            );
        }

        private readonly record struct Core(
            int TotalRatings,
            int TotalLikesReceived,
            double AverageScore,
            string? FavoriteGenre,
            int RatedMediaCount,
            int? HighestScore,
            int? LowestScore,
            DateTime? LastRatedAt
        );
    }
}
