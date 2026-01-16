using MRP.Server.Validation;

namespace MRP.Server.Models
{
    public enum MediaType
    {
        Movie,
        Series,
        Game
    }

    public sealed class MediaEntry
    {
        public string Title { get; }
        public string Description { get; }
        public int ReleaseYear { get; }
        public List<string> Genres { get; }
        public int AgeRestriction { get; }
        public MediaType Type { get; }
        public string CreatedBy { get; }
        public DateTime CreatedAt { get; }
        public DateTime LastModifiedAt { get; }

        // 🔒 interner Ctor – nur innerhalb Models / Repositories
        internal MediaEntry(
            string title,
            string description,
            int releaseYear,
            List<string> genres,
            int ageRestriction,
            MediaType type,
            string createdBy)
        {
            Title = title.Trim().ToLowerInvariant();
            Description = description.Trim();
            ReleaseYear = releaseYear;
            Genres = genres;
            AgeRestriction = ageRestriction;
            Type = type;
            CreatedBy = createdBy;

            CreatedAt = DateTime.UtcNow;
            LastModifiedAt = CreatedAt;
        }
        public static MediaEntry Create(
            string title,
            string description,
            int releaseYear,
            List<string> genres,
            int ageRestriction,
            MediaType type,
            string createdBy)
        {
            MediaValidator.ValidateForCreate(
                title,
                description,
                releaseYear,
                genres,
                ageRestriction,
                type);

            return new MediaEntry(
                title,
                description,
                releaseYear,
                genres,
                ageRestriction,
                type,
                createdBy
            );
        }

        public static MediaEntry FromDatabase(
            string title,
            string description,
            int releaseYear,
            List<string> genres,
            int ageRestriction,
            MediaType type,
            string createdBy,
            DateTime createdAt,
            DateTime lastModifiedAt)
        {
            return new MediaEntry(
                title,
                description,
                releaseYear,
                genres,
                ageRestriction,
                type,
                createdBy,
                createdAt,
                lastModifiedAt
            );
        }
        private MediaEntry(
            string title,
            string description,
            int releaseYear,
            List<string> genres,
            int ageRestriction,
            MediaType type,
            string createdBy,
            DateTime createdAt,
            DateTime lastModifiedAt)
        {
            Title = title;
            Description = description;
            ReleaseYear = releaseYear;
            Genres = genres;
            AgeRestriction = ageRestriction;
            Type = type;
            CreatedBy = createdBy;
            CreatedAt = createdAt;
            LastModifiedAt = lastModifiedAt;
        }
        public static MediaEntry UpdatedFromExisting(
            MediaEntry existing,
            string? newTitle,
            string description,
            int releaseYear,
            List<string> genres,
            int ageRestriction,
            MediaType type)
        {
            if (existing is null)
            {
                throw new ArgumentNullException(nameof(existing));
            }

            var finalTitle = string.IsNullOrWhiteSpace(newTitle)
                ? existing.Title
                : newTitle;

            MediaValidator.ValidateForCreate(
                finalTitle,
                description,
                releaseYear,
                genres,
                ageRestriction,
                type);

            return new MediaEntry(
                title: finalTitle.Trim().ToLowerInvariant(),
                description: description.Trim(),
                releaseYear: releaseYear,
                genres: genres.Select(g => g?.Trim()!).ToList(),
                ageRestriction: ageRestriction,
                type: type,
                createdBy: existing.CreatedBy,
                createdAt: existing.CreatedAt,
                lastModifiedAt: DateTime.UtcNow
            );
        }
    }
}
