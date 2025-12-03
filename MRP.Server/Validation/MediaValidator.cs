using MRP.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRP.Server.Validation
{
    public static class MediaValidator
    {
        public static void ValidateForCreate(
            string title,
            string description,
            int releaseYear,
            List<string> genres,
            int ageRestriction,
            MediaType type)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Title is not allowed to be empty or contain only whitespace.", nameof(title));
            }

            string trimmedTitle = title.Trim();

            if (trimmedTitle.Length < 3 || trimmedTitle.Length > 150)
            {
                throw new ArgumentException("Title length must be between 3 and 150 characters.", nameof(title));
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                throw new ArgumentException("Description is not allowed to be empty or contain only whitespace.", nameof(description));
            }

            string trimmedDescription = description.Trim();

            if (trimmedDescription.Length < 10 || trimmedDescription.Length > 2000)
            {
                throw new ArgumentException("Description length must be between 10 and 2000 characters.", nameof(description));
            }

            if (genres == null || genres.Count == 0)
            {
                throw new ArgumentException("Genre list must not be empty.", nameof(genres));
            }

            if (genres.Count > 5)
            {
                throw new ArgumentException("A maximum of 5 genres is allowed.", nameof(genres));
            }

            var normalized = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var g in genres)
            {
                if (string.IsNullOrWhiteSpace(g))
                {
                    throw new ArgumentException("Genres must not contain null, empty, or whitespace-only values.", nameof(genres));
                }

                string trimmedGenre = g.Trim();

                if (trimmedGenre.Length < 2 || trimmedGenre.Length > 40)
                {
                    throw new ArgumentException("Each genre must be between 2 and 40 characters long.", nameof(genres));
                }

                if (!normalized.Add(trimmedGenre))
                {
                    throw new ArgumentException("Duplicate genres are not allowed (case-insensitive).", nameof(genres));
                }
            }

            if (ageRestriction < 0 || ageRestriction > 21)
            {
                throw new ArgumentException("Age restriction must be between 0 and 21.", nameof(ageRestriction));
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
        }
    }
}