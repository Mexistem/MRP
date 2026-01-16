using MRP.Server.Models;

namespace MRP.Server.Services
{
    public interface IFavoriteManager
    {
        void AddFavorite(string username, string mediaTitle);
        void RemoveFavorite(string username, string mediaTitle);
        FavoriteList GetFavorites(string username);
    }
}
