using System;
using System.Collections.Generic;
using System.Linq;
using MRP.Server.Models;
using MRP.Server.Storage.Interfaces;

namespace MRP.Server.Storage.InMemory
{
    public sealed class InMemoryMediaRepository : IMediaRepository
    {
        private readonly List<MediaEntry> _entries = new List<MediaEntry>();

        private IRatingRepository? _ratingRepository;
        private InMemoryFavoriteRepository? _favoriteRepository;
        private InMemoryLikeRepository? _likeRepository;

        public InMemoryMediaRepository()
        {
            _ratingRepository = null;
            _favoriteRepository = null;
            _likeRepository = null;
        }

        public InMemoryMediaRepository(
            IRatingRepository ratingRepository,
            InMemoryFavoriteRepository favoriteRepository,
            InMemoryLikeRepository likeRepository)
        {
            _ratingRepository = ratingRepository ?? throw new ArgumentNullException(nameof(ratingRepository));
            _favoriteRepository = favoriteRepository ?? throw new ArgumentNullException(nameof(favoriteRepository));
            _likeRepository = likeRepository ?? throw new ArgumentNullException(nameof(likeRepository));
        }

        private static string NormTitle(string s)
        {
            return (s ?? string.Empty).Trim().ToLowerInvariant();
        }

        public IEnumerable<MediaEntry> GetAll()
        {
            return _entries;
        }

        public void Add(MediaEntry entry)
        {
            if (entry is null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            var normalizedTitle = NormTitle(entry.Title);

            bool exists = _entries.Any(m => NormTitle(m.Title) == normalizedTitle);
            if (exists)
            {
                throw new InvalidOperationException("A media entry with this title already exists.");
            }

            _entries.Add(entry);
        }

        public bool ExistsByTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return false;
            }

            var t = NormTitle(title);
            return _entries.Any(m => NormTitle(m.Title) == t);
        }

        public MediaEntry? GetByTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            var t = NormTitle(title);
            return _entries.FirstOrDefault(m => NormTitle(m.Title) == t);
        }

        public void Update(MediaEntry entry)
        {
            if (entry is null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            var t = NormTitle(entry.Title);
            var index = _entries.FindIndex(m => NormTitle(m.Title) == t);

            if (index < 0)
            {
                throw new KeyNotFoundException("Media entry not found.");
            }

            _entries[index] = entry;
        }

        public void Delete(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Title is required.", nameof(title));
            }

            var t = NormTitle(title);
            var index = _entries.FindIndex(m => NormTitle(m.Title) == t);

            if (index < 0)
            {
                throw new KeyNotFoundException("Media entry not found.");
            }

            if (_likeRepository is not null)
            {
                _likeRepository.DeleteByMediaTitle(t);
            }

            if (_favoriteRepository is not null)
            {
                _favoriteRepository.DeleteByMediaTitle(t);
            }

            if (_ratingRepository is not null)
            {
                _ratingRepository.DeleteByMediaTitle(t);
            }

            _entries.RemoveAt(index);
        }

        public void Rename(string oldTitle, MediaEntry renamedEntry)
        {
            if (string.IsNullOrWhiteSpace(oldTitle))
            {
                throw new ArgumentException("Old title is required.", nameof(oldTitle));
            }

            if (renamedEntry is null)
            {
                throw new ArgumentNullException(nameof(renamedEntry));
            }

            var oldT = NormTitle(oldTitle);
            var newT = NormTitle(renamedEntry.Title);

            var idx = _entries.FindIndex(m => NormTitle(m.Title) == oldT);
            if (idx < 0)
            {
                throw new KeyNotFoundException("Media entry not found.");
            }

            if (_entries.Any(m => NormTitle(m.Title) == newT && NormTitle(m.Title) != oldT))
            {
                throw new InvalidOperationException("Title already exists");
            }

            _entries[idx] = renamedEntry;

            if (oldT != newT)
            {
                if (_ratingRepository is not null)
                {
                    _ratingRepository.RenameMediaTitle(oldT, newT);
                }

                if (_favoriteRepository is not null)
                {
                    _favoriteRepository.RenameMediaTitle(oldT, newT);
                }

                if (_likeRepository is not null)
                {
                    _likeRepository.RenameMediaTitle(oldT, newT);
                }
            }
        }
        public void SetDependencies(
                    IRatingRepository ratingRepository,
                    InMemoryFavoriteRepository favoriteRepository,
                    InMemoryLikeRepository likeRepository)
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

            _ratingRepository = ratingRepository;
            _favoriteRepository = favoriteRepository;
            _likeRepository = likeRepository;
        }

    }
}
