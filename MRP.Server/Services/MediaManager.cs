using MRP.Server.Models;
using MRP.Server.Storage.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRP.Server.Services
{
    public class MediaManager
    {
        private readonly InMemoryMediaRepository _repository;

        public MediaManager(InMemoryMediaRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
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
            string normalizedTitle = title.Trim().ToLowerInvariant();

            var existing = _repository
                .GetAll()
                .FirstOrDefault(m => m.Title.Trim().ToLowerInvariant() == normalizedTitle);

            if (existing != null)
            {
                throw new InvalidOperationException("A media entry with this title already exists.");
            }

            var entry = new MediaEntry(
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
    }
}