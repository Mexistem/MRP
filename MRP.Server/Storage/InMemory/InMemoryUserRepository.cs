using System;
using System.Collections.Generic;
using System.Linq;
using MRP.Server.Models;
using MRP.Server.Storage.Interfaces;

namespace MRP.Server.Storage.InMemory
{
    public sealed class InMemoryUserRepository : IUserRepository
    {
        private readonly Dictionary<string, User> _users =
            new(StringComparer.OrdinalIgnoreCase);

        private IRatingRepository? _ratingRepository;
        private InMemoryFavoriteRepository? _favoriteRepository;
        private ILikeRepository? _likeRepository;
        private IMediaRepository? _mediaRepository;

        public InMemoryUserRepository()
        {
            _ratingRepository = null;
            _favoriteRepository = null;
            _likeRepository = null;
            _mediaRepository = null;
        }

        public InMemoryUserRepository(
            IRatingRepository ratingRepository,
            InMemoryFavoriteRepository favoriteRepository,
            ILikeRepository likeRepository,
            IMediaRepository mediaRepository)
        {
            _ratingRepository = ratingRepository ?? throw new ArgumentNullException(nameof(ratingRepository));
            _favoriteRepository = favoriteRepository ?? throw new ArgumentNullException(nameof(favoriteRepository));
            _likeRepository = likeRepository ?? throw new ArgumentNullException(nameof(likeRepository));
            _mediaRepository = mediaRepository ?? throw new ArgumentNullException(nameof(mediaRepository));
        }

        private static string Norm(string s)
        {
            return (s ?? "").Trim();
        }

        public bool Exists(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }

            return _users.ContainsKey(Norm(username));
        }

        public User? Get(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            return _users.TryGetValue(Norm(username), out var user) ? user : null;
        }

        public void Add(User user)
        {
            if (user is null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var u = Norm(user.Username);

            if (string.IsNullOrWhiteSpace(u))
            {
                throw new ArgumentException("Username is required.", nameof(user));
            }

            if (_users.ContainsKey(u))
            {
                throw new InvalidOperationException("User already exists.");
            }

            _users[u] = user;
        }

        public IEnumerable<User> GetAll()
        {
            return _users.Values.ToList();
        }

        public void Delete(string username)
        {
            var u = Norm(username);

            if (string.IsNullOrWhiteSpace(u))
            {
                return;
            }

            if (!_users.Remove(u))
            {
                return;
            }

            if (_mediaRepository is not null)
            {
                var titles = _mediaRepository.GetAll()
                    .Where(m => string.Equals(m.CreatedBy, u, StringComparison.OrdinalIgnoreCase))
                    .Select(m => m.Title)
                    .ToList();

                foreach (var title in titles)
                {
                    _mediaRepository.Delete(title);
                }
            }

            if (_ratingRepository is not null)
            {
                _ratingRepository.DeleteByUsername(u);
            }

            if (_favoriteRepository is not null)
            {
                _favoriteRepository.DeleteByUsername(u);
            }

            if (_likeRepository is not null)
            {
                _likeRepository.DeleteByUser(u);
            }
        }

        public void SetDependencies(
            IRatingRepository ratingRepository,
            InMemoryFavoriteRepository favoriteRepository,
            ILikeRepository likeRepository,
            IMediaRepository mediaRepository)
        {
            if (ratingRepository == null)
            {
                throw new ArgumentNullException(nameof(ratingRepository));
            }

            if (favoriteRepository == null)
            {
                throw new ArgumentNullException(nameof(favoriteRepository));
            }

            if (likeRepository == null)
            {
                throw new ArgumentNullException(nameof(likeRepository));
            }

            if (mediaRepository == null)
            {
                throw new ArgumentNullException(nameof(mediaRepository));
            }

            _ratingRepository = ratingRepository;
            _favoriteRepository = favoriteRepository;
            _likeRepository = likeRepository;
            _mediaRepository = mediaRepository;
        }
    }
}
