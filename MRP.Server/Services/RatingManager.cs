using MRP.Server.Models;
using MRP.Server.Services;
using MRP.Server.Storage.Interfaces;
using MRP.Server.Validation;
using System;


namespace MRP.Server.Services
{
    public sealed class RatingManager : IRatingManager
    {
        private readonly IRatingRepository _ratingRepository;

        public RatingManager(IRatingRepository ratingRepository)
        {
            _ratingRepository = ratingRepository ?? throw new ArgumentNullException(nameof(ratingRepository));
        }

        public RatingEntry CreateRating(string mediaTitle, string username, int value, string? comment)
        {
            mediaTitle = mediaTitle.Trim().ToLower();
            username = username.Trim();

            RatingValidator.ValidateForCreate(mediaTitle, username, value, comment);

            var existing = _ratingRepository.GetByMediaTitleAndUsername(mediaTitle, username);

            if (existing is not null)
            {
                throw new InvalidOperationException("Rating for this user already exists for this media.");
            }

            var rating = new RatingEntry(
                mediaTitle,
                username,
                value,
                comment);

            _ratingRepository.Add(rating);
            return rating;
        }

        public IEnumerable<RatingEntry> GetAllRatings()
        {
            return _ratingRepository.GetAll();
        }

        public IEnumerable<RatingEntry> GetRatingsForMedia(string mediaTitle)
        {
            if (string.IsNullOrWhiteSpace(mediaTitle))
            { 
            throw new ArgumentException("Media title is required.", nameof(mediaTitle));
            }   

            return _ratingRepository.GetByMediaTitle(mediaTitle.Trim());
        }

        public RatingEntry GetRatingForUser(string mediaTitle, string username)
        {
            if (string.IsNullOrWhiteSpace(mediaTitle))
            {
                throw new ArgumentException("Media title is required.", nameof(mediaTitle));
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username is required.", nameof(username));
            }

            mediaTitle = mediaTitle.Trim();
            username = username.Trim();

            var existing = _ratingRepository.GetByMediaTitleAndUsername(mediaTitle, username);

            if (existing is null)
            {
                throw new KeyNotFoundException("Rating not found.");
            }

            return existing;
        }

        public RatingEntry UpdateRating(string mediaTitle, string username, int value, string? comment)
        {
            RatingValidator.ValidateForCreate(mediaTitle, username, value, comment);

            mediaTitle = mediaTitle.Trim().ToLower();
            username = username.Trim();

            var existing = _ratingRepository.GetByMediaTitleAndUsername(mediaTitle, username);

            if (existing is null)
            {
                throw new KeyNotFoundException("Rating not found.");
            }


            var updated = RatingEntry.FromDatabase(
                mediaTitle,
                username,
                value,
                comment,
                existing.CreatedAt);

            _ratingRepository.Update(updated);
            return updated;
        }

        public double GetAverageRatingForMedia(string mediaTitle)
        {
            var ratings = _ratingRepository
                .GetAll()
                .Where(r =>
                    r.MediaTitle.Equals(mediaTitle, StringComparison.OrdinalIgnoreCase) &&
                    r.Value >= 1 &&
                    r.Value <= 5)
                .ToList();

            if (ratings.Count == 0)
                return 0;

            return ratings.Average(r => r.Value);
        }

        public void DeleteRating(string mediaTitle, string username)
        {
            if (string.IsNullOrWhiteSpace(mediaTitle))
            {
                throw new ArgumentException("Media title is required.", nameof(mediaTitle));
            }

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username is required.", nameof(username));
            }


            var deleted = _ratingRepository.DeleteRating(mediaTitle, username);
            if (!deleted)
            {
                throw new ArgumentException("Rating not found.");
            }
        }

    }
}

