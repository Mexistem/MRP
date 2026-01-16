using MRP.Server.Models;
using MRP.Server.Storage.Interfaces;
using MRP.Server.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MRP.Server.Services
{
    public sealed class MediaManager : IMediaManager
    {
        private readonly IMediaRepository _repository;
        private readonly IUserManager _userManager;

        public MediaManager(IMediaRepository repository, IUserManager userManager)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

        public MediaEntry CreateMedia(
            string title,
            string description,
            int releaseYear,
            List<string> genres,
            int ageRestriction,
            MediaType type,
            string createdBy)
        {
            title = title.Trim().ToLowerInvariant();
            description = description.Trim();
            createdBy = createdBy.Trim();

            genres = genres.Select(g => g?.Trim()!).ToList();

            MediaValidator.ValidateForCreate(
                title,
                description,
                releaseYear,
                genres,
                ageRestriction,
                type);

            if (_repository.ExistsByTitle(title))
            {
                throw new InvalidOperationException("Title already exists");
            }

            var entry = MediaEntry.Create(
                title,
                description,
                releaseYear,
                genres,
                ageRestriction,
                type,
                createdBy);

            _repository.Add(entry);
            return entry;
        }

        public MediaEntry UpdateMedia(
            string title,
            string? newTitle,
            string description,
            int releaseYear,
            List<string> genres,
            int ageRestriction,
            MediaType type,
            string requestedBy)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Title is required.", nameof(title));
            }

            if (string.IsNullOrWhiteSpace(requestedBy))
            {
                throw new ArgumentException("RequestedBy is required.", nameof(requestedBy));
            }

            var oldTitle = title.Trim().ToLowerInvariant();
            requestedBy = requestedBy.Trim();
            description = description.Trim();
            genres = genres.Select(g => g?.Trim()!).ToList();

            var existing = _repository.GetByTitle(oldTitle)
                ?? throw new KeyNotFoundException($"Media '{oldTitle}' not found.");

            var isAdmin = _userManager.IsAdmin(requestedBy);
            var isCreator = string.Equals(existing.CreatedBy, requestedBy, StringComparison.OrdinalIgnoreCase);

            if (!isAdmin && !isCreator)
            {
                throw new UnauthorizedAccessException("Only the creator or an admin can update media.");
            }

            var finalTitle = existing.Title;
            var isRenaming = false;

            if (newTitle is not null)
            {
                var normalizedNewTitle = newTitle.Trim().ToLowerInvariant();

                if (!string.Equals(normalizedNewTitle, existing.Title, StringComparison.OrdinalIgnoreCase)
                    && _repository.ExistsByTitle(normalizedNewTitle))
                {
                    throw new InvalidOperationException("Title already exists");
                }

                finalTitle = normalizedNewTitle;
                isRenaming = !string.Equals(finalTitle, existing.Title, StringComparison.OrdinalIgnoreCase);
            }

            MediaValidator.ValidateForCreate(
                finalTitle,
                description,
                releaseYear,
                genres,
                ageRestriction,
                type);

            var updated = MediaEntry.UpdatedFromExisting(
                existing,
                isRenaming ? finalTitle : null,
                description,
                releaseYear,
                genres,
                ageRestriction,
                type);

            if (isRenaming)
            {
                _repository.Rename(existing.Title, updated);
            }
            else
            {
                _repository.Update(updated);
            }

            return updated;
        }

        public void DeleteMedia(string title, string requestedBy)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Title is required.", nameof(title));
            }

            if (string.IsNullOrWhiteSpace(requestedBy))
            {
                throw new ArgumentException("RequestedBy is required.", nameof(requestedBy));
            }

            var t = title.Trim().ToLowerInvariant();
            requestedBy = requestedBy.Trim();

            var existing = _repository.GetByTitle(t)
                ?? throw new KeyNotFoundException("Media not found");

            var isAdmin = _userManager.IsAdmin(requestedBy);
            var isCreator = string.Equals(existing.CreatedBy, requestedBy, StringComparison.OrdinalIgnoreCase);

            if (!isAdmin && !isCreator)
            {
                throw new UnauthorizedAccessException("Only the creator or an admin can delete media.");
            }

            _repository.Delete(t);
        }

        public MediaEntry? GetByTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            return _repository.GetByTitle(title.Trim().ToLowerInvariant());
        }

        public IEnumerable<MediaEntry> GetAll()
        {
            return _repository.GetAll();
        }

        public bool Exists(string title)
        {
            return !string.IsNullOrWhiteSpace(title)
                && _repository.ExistsByTitle(title.Trim().ToLowerInvariant());
        }
    }
}
