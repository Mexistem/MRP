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

        public MediaEntry(
            string title,
            string description,
            int releaseYear,
            List<string> genres,
            int ageRestriction,
            MediaType type,
            string createdBy)
        {
            string trimmedTitle = title.Trim();
            string trimmedDescription = description.Trim();

            MediaValidator.ValidateForCreate(
                trimmedTitle,
                trimmedDescription,
                releaseYear,
                genres,
                ageRestriction,
                type);

            for (int i = 0; i < genres.Count; i++)
            {
                genres[i] = genres[i].Trim();
            }

            Title = trimmedTitle;
            Description = trimmedDescription;
            ReleaseYear = releaseYear;
            Genres = genres;
            AgeRestriction = ageRestriction;
            Type = type;
            CreatedBy = createdBy;

            var now = DateTime.UtcNow;
            CreatedAt = now;
            LastModifiedAt = now;
        }
    }
}
