using MRP.Server.Models;
using MRP.Server.Storage;

namespace MRP.Server.Services
{
    public class RatingManager : IRatingManager
    {
        private readonly IRatingRepository _repository;

        public RatingManager(IRatingRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public RatingEntry CreateRating(
            string mediaTitle,
            string username,
            int value,
            string? comment)
        {
            var rating = new RatingEntry(
                mediaTitle,
                username,
                value,
                comment);

            _repository.Add(rating);

            return rating;
        }

        public IEnumerable<RatingEntry> GetAllRatings()
        {
            return _repository.GetAll();
        }

        public IEnumerable<RatingEntry> GetRatingsForMedia(string mediaTitle)
        {
            return _repository.GetByMediaTitle(mediaTitle);
        }
    }
}