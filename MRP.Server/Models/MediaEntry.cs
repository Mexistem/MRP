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

            if (ageRestriction < 0 || ageRestriction > 21)
            {
                throw new ArgumentException("Age restriction must be between 0 and 21.", nameof(ageRestriction));
            }

            for (int i = 0; i < genres.Count; i++)
            {
                genres[i] = genres[i].Trim();
            }

            if (genres.Count > 5)
            {
                throw new ArgumentException("A maximum of 5 genres is allowed.", nameof(genres));
            }

            foreach (var g in genres)
            {
                if (g.Length > 40)
                {
                    throw new ArgumentException("Each genre must be under 40 characters long.", nameof(genres));
                }
            }

            var normalized = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var g in genres)
            {
                if (!normalized.Add(g))
                {
                    throw new ArgumentException("Duplicate genres are not allowed (case-insensitive).", nameof(genres));
                }
            }

            int maxYear = DateTime.Now.Year + 1;

            if (releaseYear < 1900 || releaseYear > maxYear)
            {
                throw new ArgumentException("Release year must be between 1900 and next year.", nameof(releaseYear));
            }

            if (!Enum.IsDefined(typeof(MediaType), type))
            {
                throw new ArgumentException("Invalid media type.", nameof(type));
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
