using System.Collections.Generic;
using MRP.Server.Models;

namespace MRP.Server.Services
{
    public interface IRatingManager
    {
        RatingEntry CreateRating(
            string mediaTitle,
            string username,
            int value,
            string? comment);

        IEnumerable<RatingEntry> GetAllRatings();
        IEnumerable<RatingEntry> GetRatingsForMedia(string mediaTitle);
    }
}