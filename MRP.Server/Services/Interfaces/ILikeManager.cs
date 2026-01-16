using MRP.Server.Models;

namespace MRP.Server.Services
{
    public interface ILikeManager
    {
        LikeInfo LikeRating(string mediaTitle, string ratingUsername, string likedBy);
        LikeInfo UnlikeRating(string mediaTitle, string ratingUsername, string likedBy);
        int GetLikeCount(string mediaTitle, string ratingUsername);
    }
}
