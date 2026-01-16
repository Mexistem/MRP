using MRP.Server.Models;
using MRP.Server.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MRP.Server.Services
{
    public sealed class LeaderboardManager : ILeaderboardManager
    {
        private readonly IUserManager _userManager;
        private readonly IRatingManager _ratingManager;
        private readonly ILikeManager _likeManager;
        private readonly IMediaManager _mediaManager;

        public LeaderboardManager(
            IUserManager userManager,
            IRatingManager ratingManager,
            ILikeManager likeManager,
            IMediaManager mediaManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _ratingManager = ratingManager ?? throw new ArgumentNullException(nameof(ratingManager));
            _likeManager = likeManager ?? throw new ArgumentNullException(nameof(likeManager));
            _mediaManager = mediaManager ?? throw new ArgumentNullException(nameof(mediaManager));
        }

        public IEnumerable<LeaderboardEntry> GetLeaderboard()
        {
            var allUsers = _userManager.GetAllUsers().ToList();
            var allRatings = _ratingManager.GetAllRatings().ToList();

            var entries = allUsers.Select(u =>
            {
                var userRatingsAll = allRatings
                    .Where(r => string.Equals(r.Username, u.Username, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var userRatings = userRatingsAll
                    .Where(r => _mediaManager.GetByTitle(r.MediaTitle) is not null)
                    .ToList();

                int totalRatings = userRatings.Count;

                int totalLikesReceived = userRatings.Sum(r =>
                    _likeManager.GetLikeCount(r.MediaTitle, r.Username));

                double averageScore = 0;
                if (totalRatings > 0)
                {
                    averageScore = userRatings.Average(r => r.Value);
                }

                return new LeaderboardEntry
                {
                    Username = u.Username,
                    TotalRatings = totalRatings,
                    TotalLikesReceived = totalLikesReceived,
                    AverageScore = averageScore
                };
            })
            .OrderByDescending(x => x.TotalRatings)
            .ThenByDescending(x => x.TotalLikesReceived)
            .ThenByDescending(x => x.AverageScore)
            .ThenBy(x => x.Username, StringComparer.OrdinalIgnoreCase)
            .ToList();

            return entries;
        }
    }
}
