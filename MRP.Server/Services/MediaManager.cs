using MRP.Server.Models;
using MRP.Server.Storage.InMemory;

namespace MRP.Server.Services
{
    public class MediaManager : IMediaManager

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