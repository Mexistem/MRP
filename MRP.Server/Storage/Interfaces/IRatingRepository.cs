using System.Collections.Generic;
using MRP.Server.Models;

namespace MRP.Server.Storage
{
    public interface IRatingRepository
    {
        IEnumerable<RatingEntry> GetAll();
        IEnumerable<RatingEntry> GetByMediaTitle(string mediaTitle);
        void Add(RatingEntry rating);
    }
}