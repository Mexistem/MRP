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

            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Title is not allowed to be empty or contain only whitespace", nameof(title));
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException("Description is not allowed to be empty or contain only whitespace.", nameof(description));
            }

            if (trimmedTitle.Length > 150)
            {
                throw new ArgumentException("Title length must be under 150 characters.", nameof(title));
            }

            Title = trimmedTitle;
            Description = trimmedDescription;
            ReleaseYear = releaseYear;
            Genres = genres;
            AgeRestriction = ageRestriction;
            Type = type;
            CreatedBy = createdBy;
        }
    }
}
