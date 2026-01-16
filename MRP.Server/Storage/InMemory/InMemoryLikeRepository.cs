using System;
using System.Collections.Generic;
using System.Linq;
using MRP.Server.Storage.Interfaces;

namespace MRP.Server.Storage.InMemory
{
    public sealed class InMemoryLikeRepository : ILikeRepository
    {
        private readonly Dictionary<(string MediaTitle, string RatingUsername), HashSet<string>> _likes = new();

        private IRatingRepository? _ratingRepository;
        private IUserRepository? _userRepository;

        public InMemoryLikeRepository()
        {
            _ratingRepository = null;
            _userRepository = null;
        }

        public InMemoryLikeRepository(IRatingRepository ratingRepository, IUserRepository userRepository)
        {
            _ratingRepository = ratingRepository ?? throw new ArgumentNullException(nameof(ratingRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        private static string Norm(string s)
        {
            return (s ?? "").Trim().ToLowerInvariant();
        }

        private static (string MediaTitle, string RatingUsername) Key(string mediaTitle, string ratingUsername)
        {
            return (Norm(mediaTitle), Norm(ratingUsername));
        }

        public bool Exists(string mediaTitle, string ratingUsername, string likedBy)
        {
            if (string.IsNullOrWhiteSpace(mediaTitle) ||
                string.IsNullOrWhiteSpace(ratingUsername) ||
                string.IsNullOrWhiteSpace(likedBy))
            {
                return false;
            }

            var key = Key(mediaTitle, ratingUsername);
            return _likes.TryGetValue(key, out var set) && set.Contains(Norm(likedBy));
        }

        public void Add(string mediaTitle, string ratingUsername, string likedBy)
        {
            if (string.IsNullOrWhiteSpace(mediaTitle))
            {
                throw new ArgumentException("mediaTitle is required", nameof(mediaTitle));
            }

            if (string.IsNullOrWhiteSpace(ratingUsername))
            {
                throw new ArgumentException("ratingUsername is required", nameof(ratingUsername));
            }

            if (string.IsNullOrWhiteSpace(likedBy))
            {
                throw new ArgumentException("likedBy is required", nameof(likedBy));
            }

            var t = Norm(mediaTitle);
            var ru = Norm(ratingUsername);
            var lb = Norm(likedBy);

            if (_userRepository is not null)
            {
                if (!_userRepository.Exists(ru))
                {
                    throw new KeyNotFoundException("User not found.");
                }

                if (!_userRepository.Exists(lb))
                {
                    throw new KeyNotFoundException("User not found.");
                }
            }

            if (_ratingRepository is not null)
            {
                var rating = _ratingRepository.GetByMediaTitleAndUsername(t, ru);
                if (rating is null)
                {
                    throw new KeyNotFoundException("Rating not found.");
                }
            }

            var key = (MediaTitle: t, RatingUsername: ru);

            if (!_likes.TryGetValue(key, out var set))
            {
                set = new HashSet<string>();
                _likes[key] = set;
            }

            if (!set.Add(lb))
            {
                throw new InvalidOperationException("Like already exists");
            }
        }

        public void Remove(string mediaTitle, string ratingUsername, string likedBy)
        {
            if (string.IsNullOrWhiteSpace(mediaTitle) ||
                string.IsNullOrWhiteSpace(ratingUsername) ||
                string.IsNullOrWhiteSpace(likedBy))
            {
                return;
            }

            var key = Key(mediaTitle, ratingUsername);

            if (!_likes.TryGetValue(key, out var set))
            {
                return;
            }

            set.Remove(Norm(likedBy));

            if (set.Count == 0)
            {
                _likes.Remove(key);
            }
        }

        public int CountForRating(string mediaTitle, string ratingUsername)
        {
            if (string.IsNullOrWhiteSpace(mediaTitle) || string.IsNullOrWhiteSpace(ratingUsername))
            {
                return 0;
            }

            var key = Key(mediaTitle, ratingUsername);
            return _likes.TryGetValue(key, out var set) ? set.Count : 0;
        }

        public void DeleteByMediaTitle(string mediaTitle)
        {
            if (string.IsNullOrWhiteSpace(mediaTitle))
            {
                return;
            }

            var t = Norm(mediaTitle);

            var keysToRemove = _likes.Keys
                .Where(k => k.MediaTitle == t)
                .ToList();

            foreach (var k in keysToRemove)
            {
                _likes.Remove(k);
            }
        }

        public void DeleteByUser(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return;
            }

            var u = Norm(username);
            var keys = _likes.Keys.ToList();

            foreach (var key in keys)
            {
                if (key.RatingUsername == u)
                {
                    _likes.Remove(key);
                    continue;
                }

                var set = _likes[key];
                set.Remove(u);

                if (set.Count == 0)
                {
                    _likes.Remove(key);
                }
            }
        }

        public void DeleteForRating(string mediaTitle, string ratingUsername)
        {
            if (string.IsNullOrWhiteSpace(mediaTitle))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(ratingUsername))
            {
                return;
            }

            var key = Key(mediaTitle, ratingUsername);
            _likes.Remove(key);
        }

        public void RenameMediaTitle(string oldTitle, string newTitle)
        {
            if (string.IsNullOrWhiteSpace(oldTitle) || string.IsNullOrWhiteSpace(newTitle))
            {
                return;
            }

            var oldT = Norm(oldTitle);
            var newT = Norm(newTitle);

            if (oldT == newT)
            {
                return;
            }

            var keysToMove = _likes.Keys
                .Where(k => k.MediaTitle == oldT)
                .ToList();

            foreach (var oldKey in keysToMove)
            {
                if (!_likes.TryGetValue(oldKey, out var set))
                {
                    continue;
                }

                var newKey = (MediaTitle: newT, RatingUsername: oldKey.RatingUsername);

                if (_likes.TryGetValue(newKey, out var existing))
                {
                    existing.UnionWith(set);
                }
                else
                {
                    _likes[newKey] = set;
                }

                _likes.Remove(oldKey);
            }
        }
        public void SetDependencies(
                    IRatingRepository ratingRepository,
                    IUserRepository userRepository)
        {
            if (ratingRepository == null)
            {
                throw new ArgumentNullException(nameof(ratingRepository));
            }

            if (userRepository == null)
            {
                throw new ArgumentNullException(nameof(userRepository));
            }

            _ratingRepository = ratingRepository;
            _userRepository = userRepository;
        }

    }
}
