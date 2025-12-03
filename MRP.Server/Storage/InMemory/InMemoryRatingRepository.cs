namespace MRP.Server.Storage.InMemory
{
    public class InMemoryRatingRepository : IRatingRepository
    {
        private readonly List<RatingEntry> _entries = new List<RatingEntry>();

        public IEnumerable<RatingEntry> GetAll()
        {
            return _entries;
        }

        public IEnumerable<RatingEntry> GetByMediaTitle(string mediaTitle)
        {
            return _entries.Where(r => r.MediaTitle == mediaTitle);
        }

        public void Add(RatingEntry rating)
        {
            if (rating == null)
            {
                throw new ArgumentNullException(nameof(rating));
            }

            _entries.Add(rating);
        }
    }
}
