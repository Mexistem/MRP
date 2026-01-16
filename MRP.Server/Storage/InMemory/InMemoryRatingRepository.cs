using System;
using System.Collections.Generic;
using System.Linq;
using MRP.Server.Models;
using MRP.Server.Storage.Interfaces;

namespace MRP.Server.Storage.InMemory
{
    public sealed class InMemoryRatingRepository : IRatingRepository
    {
        private readonly List<RatingEntry> _entries = new List<RatingEntry>();

        private InMemoryLikeRepository? _likeRepository;
        private IMediaRepository? _mediaRepository;
        private IUserRepository? _userRepository;

        public InMemoryRatingRepository()
        {
            _likeRepository = null;
            _mediaRepository = null;
            _userRepository = null;
        }

        public InMemoryRatingRepository(InMemoryLikeRepository likeRepository)
        {
            _likeRepository = likeRepository ?? throw new ArgumentNullException(nameof(likeRepository));
            _mediaRepository = null;
            _userRepository = null;
        }

        public InMemoryRatingRepository(InMemoryLikeRepository likeRepository, IMediaRepository mediaRepository, IUserRepository userRepository)
        {
            _likeRepository = likeRepository ?? throw new ArgumentNullException(nameof(likeRepository));
            _mediaRepository = mediaRepository ?? throw new ArgumentNullException(nameof(mediaRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        private static string Norm(string s)
        {
            return (s ?? string.Empty).Trim();
        }

        private static string NormTitle(string s)
        {
            return (s ?? string.Empty).Trim().ToLowerInvariant();
        }

        public IEnumerable<RatingEntry> GetAll()
        {
            return _entries;
        }

        public IEnumerable<RatingEntry> GetByMediaTitle(string mediaTitle)
        {
            var t = NormTitle(mediaTitle);
            return _entries.Where(r => r.MediaTitle.Trim().ToLowerInvariant() == t);
        }

        public RatingEntry? GetByMediaTitleAndUsername(string mediaTitle, string username)
        {
            var t = NormTitle(mediaTitle);
            var u = Norm(username);

            return _entries.FirstOrDefault(r =>
                r.MediaTitle.Trim().ToLowerInvariant() == t &&
                r.Username.Equals(u, StringComparison.OrdinalIgnoreCase));
        }

        public void Add(RatingEntry rating)
        {
            if (rating is null)
            {
                throw new ArgumentNullException(nameof(rating));
            }

            var t = NormTitle(rating.MediaTitle);
            var u = Norm(rating.Username);

            if (_userRepository is not null && !_userRepository.Exists(u))
            {
                throw new KeyNotFoundException("User not found.");
            }

            if (_mediaRepository is not null && !_mediaRepository.ExistsByTitle(t))
            {
                throw new KeyNotFoundException("Media not found.");
            }

            var exists = _entries.Any(r =>
                r.MediaTitle.Trim().ToLowerInvariant() == t &&
                r.Username.Equals(u, StringComparison.OrdinalIgnoreCase));

            if (exists)
            {
                throw new InvalidOperationException("Rating for this User already exists");
            }

            _entries.Add(rating);
        }

        public void Update(RatingEntry rating)
        {
            if (rating is null)
            {
                throw new ArgumentNullException(nameof(rating));
            }

            var t = NormTitle(rating.MediaTitle);
            var u = Norm(rating.Username);

            if (_userRepository is not null && !_userRepository.Exists(u))
            {
                throw new KeyNotFoundException("User not found.");
            }

            if (_mediaRepository is not null && !_mediaRepository.ExistsByTitle(t))
            {
                throw new KeyNotFoundException("Media not found.");
            }

            var index = _entries.FindIndex(r =>
                r.MediaTitle.Trim().ToLowerInvariant() == t &&
                r.Username.Equals(u, StringComparison.OrdinalIgnoreCase));

            if (index < 0)
            {
                throw new KeyNotFoundException("Rating does not exist and cannot be updated");
            }

            _entries[index] = rating;
        }

        public void DeleteByMediaTitle(string mediaTitle)
        {
            if (string.IsNullOrWhiteSpace(mediaTitle))
            {
                return;
            }

            var t = NormTitle(mediaTitle);

            if (_likeRepository is not null)
            {
                var ratingUsers = _entries
                    .Where(r => r.MediaTitle.Trim().ToLowerInvariant() == t)
                    .Select(r => r.Username)
                    .ToList();

                foreach (var ratingUsername in ratingUsers)
                {
                    _likeRepository.DeleteForRating(t, ratingUsername);
                }
            }

            _entries.RemoveAll(r => r.MediaTitle.Trim().ToLowerInvariant() == t);
        }

        public bool DeleteRating(string mediaTitle, string username)
        {
            if (string.IsNullOrWhiteSpace(mediaTitle))
            {
                throw new ArgumentException("Media title is required.", nameof(mediaTitle));
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username is required.", nameof(username));
            }

            var t = NormTitle(mediaTitle);
            var u = Norm(username);

            if (_likeRepository is not null)
            {
                _likeRepository.DeleteForRating(t, u);
            }

            var index = _entries.FindIndex(r =>
                r.MediaTitle.Trim().ToLowerInvariant() == t &&
                r.Username.Equals(u, StringComparison.OrdinalIgnoreCase));

            if (index < 0)
            {
                return false;
            }

            _entries.RemoveAt(index);
            return true;
        }

        public void RenameMediaTitle(string oldTitle, string newTitle)
        {
            if (string.IsNullOrWhiteSpace(oldTitle) || string.IsNullOrWhiteSpace(newTitle))
            {
                return;
            }

            var oldT = NormTitle(oldTitle);
            var newT = NormTitle(newTitle);

            if (oldT == newT)
            {
                return;
            }

            for (int i = 0; i < _entries.Count; i++)
            {
                var r = _entries[i];

                if (r.MediaTitle.Trim().ToLowerInvariant() != oldT)
                {
                    continue;
                }

                _entries[i] = RatingEntry.FromDatabase(
                    mediaTitle: newT,
                    username: r.Username,
                    value: r.Value,
                    comment: r.Comment,
                    createdAt: r.CreatedAt
                );
            }

            if (_likeRepository is not null)
            {
                _likeRepository.RenameMediaTitle(oldT, newT);
            }
        }

        public void DeleteByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return;
            }

            var u = Norm(username);

            if (_likeRepository is not null)
            {
                _likeRepository.DeleteByUser(u);
            }

            _entries.RemoveAll(r => r.Username.Equals(u, StringComparison.OrdinalIgnoreCase));
        }
        public void SetDependencies(
                    InMemoryLikeRepository likeRepository,
                    IMediaRepository mediaRepository,
                    IUserRepository userRepository)
            {
                if (likeRepository == null)
                {
                    throw new ArgumentNullException(nameof(likeRepository));
                }

                if (mediaRepository == null)
                {
                    throw new ArgumentNullException(nameof(mediaRepository));
                }

                if (userRepository == null)
                {
                    throw new ArgumentNullException(nameof(userRepository));
                }

                _likeRepository = likeRepository;
                _mediaRepository = mediaRepository;
                _userRepository = userRepository;
            }

    }
}
