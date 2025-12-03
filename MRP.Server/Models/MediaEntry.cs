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

            if (genres == null || genres.Count == 0)
            {
                throw new ArgumentException("Genre list must not be empty.", nameof(genres));
            }

            foreach (var g in genres)
            {
                if (string.IsNullOrWhiteSpace(g))
                {
                    throw new ArgumentException("Genres must not contain null, empty, or whitespace-only values.", nameof(genres));
                }
            }

            if (trimmedTitle.Length > 150)
            {
                throw new ArgumentException("Title length must be under 150 characters.", nameof(title));
            }

            if (trimmedDescription.Length < 10 || trimmedDescription.Length > 2000)
            {
                throw new ArgumentException("Description length must be between 10 and 2000 characters.", nameof(description));
            }

            for (int i = 0; i < genres.Count; i++)
            {
                genres[i] = genres[i].Trim();
            }

            if (genres.Count > 5)
            {
                throw new ArgumentException("A maximum of 5 genres is allowed.", nameof(genres));
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
