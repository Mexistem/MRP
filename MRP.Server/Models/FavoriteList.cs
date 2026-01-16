using System.Collections.Generic;

namespace MRP.Server.Models
{
    public sealed class FavoriteList
    {
        public string Username { get; }
        public IReadOnlyList<string> MediaTitles { get; }

        public FavoriteList(string username, IReadOnlyList<string> mediaTitles)
        {
            Username = username;
            MediaTitles = mediaTitles;
        }
    }
}
