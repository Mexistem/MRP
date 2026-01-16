using System;

namespace MRP.Server.Storage.Interfaces
{
    public interface ILikeRepository
    {
        bool Exists(string mediaTitle, string ratingUsername, string likedBy);
        void Add(string mediaTitle, string ratingUsername, string likedBy);
        void Remove(string mediaTitle, string ratingUsername, string likedBy);
        int CountForRating(string mediaTitle, string ratingUsername);
        void DeleteByUser(string username);
        void DeleteByMediaTitle(string mediaTitle);
    }
}
