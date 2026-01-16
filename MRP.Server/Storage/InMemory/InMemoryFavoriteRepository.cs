using System;
using System.Collections.Generic;
using System.Linq;
using MRP.Server.Storage.Interfaces;

namespace MRP.Server.Storage.InMemory
{
    public sealed class InMemoryFavoriteRepository : IFavoriteRepository
    {
        private readonly Dictionary<string, List<string>> _favoritesByUser = new();

        private IMediaRepository? _mediaRepository;
        private IUserRepository? _userRepository;

        public InMemoryFavoriteRepository()
        {
            _mediaRepository = null;
            _userRepository = null;
        }

        public InMemoryFavoriteRepository(IMediaRepository mediaRepository, IUserRepository userRepository)
        {
            _mediaRepository = mediaRepository ?? throw new ArgumentNullException(nameof(mediaRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        private static string Norm(string s)
        {
            return (s ?? "").Trim().ToLowerInvariant();
        }

        public bool Exists(string username, string mediaTitle)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(mediaTitle))
            {
                return false;
            }

            var u = Norm(username);
            var t = Norm(mediaTitle);

            return _favoritesByUser.TryGetValue(u, out var list) && list.Contains(t);
        }

        public void Add(string username, string mediaTitle)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("username is required", nameof(username));
            }

            if (string.IsNullOrWhiteSpace(mediaTitle))
            {
                throw new ArgumentException("mediaTitle is required", nameof(mediaTitle));
            }

            var u = Norm(username);
            var t = Norm(mediaTitle);

            if (_userRepository is not null && !_userRepository.Exists(u))
            {
                throw new KeyNotFoundException("User not found.");
            }

            if (_mediaRepository is not null && !_mediaRepository.ExistsByTitle(t))
            {
                throw new KeyNotFoundException("Media not found.");
            }

            if (!_favoritesByUser.TryGetValue(u, out var list))
            {
                list = new List<string>();
                _favoritesByUser[u] = list;
            }

            if (list.Contains(t))
            {
                throw new InvalidOperationException("Favorite already exists");
            }

            list.Add(t);
        }

        public void Remove(string username, string mediaTitle)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(mediaTitle))
            {
                return;
            }

            var u = Norm(username);
            var t = Norm(mediaTitle);

            if (!_favoritesByUser.TryGetValue(u, out var list))
            {
                return;
            }

            list.Remove(t);

            if (list.Count == 0)
            {
                _favoritesByUser.Remove(u);
            }
        }

        public IEnumerable<string> GetFavoriteMediaTitles(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return Array.Empty<string>();
            }

            var u = Norm(username);

            if (!_favoritesByUser.TryGetValue(u, out var list))
            {
                return Array.Empty<string>();
            }

            return list.ToList();
        }

        public void DeleteByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return;
            }

            _favoritesByUser.Remove(Norm(username));
        }

        public void DeleteByMediaTitle(string mediaTitle)
        {
            if (string.IsNullOrWhiteSpace(mediaTitle))
            {
                return;
            }

            var t = Norm(mediaTitle);

            var users = _favoritesByUser.Keys.ToList();
            foreach (var u in users)
            {
                var list = _favoritesByUser[u];
                list.RemoveAll(x => x == t);

                if (list.Count == 0)
                {
                    _favoritesByUser.Remove(u);
                }
            }
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

            var users = _favoritesByUser.Keys.ToList();
            foreach (var u in users)
            {
                var list = _favoritesByUser[u];

                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] == oldT)
                    {
                        list[i] = newT;
                    }
                }

                var distinct = list.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                _favoritesByUser[u] = distinct;

                if (distinct.Count == 0)
                {
                    _favoritesByUser.Remove(u);
                }
            }
        }
        public void SetDependencies(
                    IMediaRepository mediaRepository,
                    IUserRepository userRepository)
        {
            if (mediaRepository == null)
            {
                throw new ArgumentNullException(nameof(mediaRepository));
            }

            if (userRepository == null)
            {
                throw new ArgumentNullException(nameof(userRepository));
            }

            _mediaRepository = mediaRepository;
            _userRepository = userRepository;
        }

    }
}
