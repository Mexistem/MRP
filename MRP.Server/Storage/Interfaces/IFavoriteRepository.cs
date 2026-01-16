using System.Collections.Generic;

namespace MRP.Server.Storage.Interfaces
{
    public interface IFavoriteRepository
    {
        bool Exists(string username, string mediaTitle);
        void Add(string username, string mediaTitle);
        void Remove(string username, string mediaTitle);
        IEnumerable<string> GetFavoriteMediaTitles(string username);
        void DeleteByUsername(string username);
        void DeleteByMediaTitle(string mediaTitle);
    }
}
