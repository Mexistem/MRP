using System;
using MRP.Server.Models;
using MRP.Server.Storage.Interfaces;

namespace MRP.Server.Services
{
    public sealed class LikeManager : ILikeManager
    {
        private readonly ILikeRepository _likeRepository;

        public LikeManager(ILikeRepository likeRepository)
        {
            _likeRepository = likeRepository ?? throw new ArgumentNullException(nameof(likeRepository));
        }

        public LikeInfo LikeRating(string mediaTitle, string ratingUsername, string likedBy)
        {
            if (string.IsNullOrWhiteSpace(mediaTitle))
                throw new ArgumentException("mediaTitle is required", nameof(mediaTitle));
            if (string.IsNullOrWhiteSpace(ratingUsername))
                throw new ArgumentException("ratingUsername is required", nameof(ratingUsername));
            if (string.IsNullOrWhiteSpace(likedBy))
                throw new ArgumentException("likedBy is required", nameof(likedBy));

            mediaTitle = mediaTitle.Trim().ToLowerInvariant();
            ratingUsername = ratingUsername.Trim();
            likedBy = likedBy.Trim();

            if (_likeRepository.Exists(mediaTitle, ratingUsername, likedBy))
                throw new InvalidOperationException("Like already exists");

            _likeRepository.Add(mediaTitle, ratingUsername, likedBy);

            var count = _likeRepository.CountForRating(mediaTitle, ratingUsername);
            return new LikeInfo(mediaTitle, ratingUsername, count);
        }

        public LikeInfo UnlikeRating(string mediaTitle, string ratingUsername, string likedBy)
        {
            if (string.IsNullOrWhiteSpace(mediaTitle))
                throw new ArgumentException("mediaTitle is required", nameof(mediaTitle));
            if (string.IsNullOrWhiteSpace(ratingUsername))
                throw new ArgumentException("ratingUsername is required", nameof(ratingUsername));
            if (string.IsNullOrWhiteSpace(likedBy))
                throw new ArgumentException("likedBy is required", nameof(likedBy));

            mediaTitle = mediaTitle.Trim().ToLowerInvariant();
            ratingUsername = ratingUsername.Trim();
            likedBy = likedBy.Trim();

            _likeRepository.Remove(mediaTitle, ratingUsername, likedBy);

            var count = _likeRepository.CountForRating(mediaTitle, ratingUsername);
            return new LikeInfo(mediaTitle, ratingUsername, count);
        }

        public int GetLikeCount(string mediaTitle, string ratingUsername)
        {
            if (string.IsNullOrWhiteSpace(mediaTitle))
                throw new ArgumentException("mediaTitle is required", nameof(mediaTitle));
            if (string.IsNullOrWhiteSpace(ratingUsername))
                throw new ArgumentException("ratingUsername is required", nameof(ratingUsername));

            mediaTitle = mediaTitle.Trim().ToLowerInvariant();
            ratingUsername = ratingUsername.Trim();

            return _likeRepository.CountForRating(mediaTitle, ratingUsername);
        }
    }
}
