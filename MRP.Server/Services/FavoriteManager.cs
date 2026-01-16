using System;
using System.Linq;
using MRP.Server.Models;
using MRP.Server.Storage.Interfaces;

namespace MRP.Server.Services
{
    public sealed class FavoriteManager : IFavoriteManager
    {
        private readonly IFavoriteRepository _favoriteRepository;

        public FavoriteManager(IFavoriteRepository favoriteRepository)
        {
            _favoriteRepository = favoriteRepository ?? throw new ArgumentNullException(nameof(favoriteRepository));
        }

        public void AddFavorite(string username, string mediaTitle)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("username is required", nameof(username));
            if (string.IsNullOrWhiteSpace(mediaTitle))
                throw new ArgumentException("mediaTitle is required", nameof(mediaTitle));

            username = username.Trim();
            mediaTitle = mediaTitle.Trim().ToLowerInvariant();

            if (_favoriteRepository.Exists(username, mediaTitle))
                throw new InvalidOperationException("Favorite already exists");

            _favoriteRepository.Add(username, mediaTitle);
        }

        public void RemoveFavorite(string username, string mediaTitle)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("username is required", nameof(username));
            if (string.IsNullOrWhiteSpace(mediaTitle))
                throw new ArgumentException("mediaTitle is required", nameof(mediaTitle));

            username = username.Trim();
            mediaTitle = mediaTitle.Trim().ToLowerInvariant();

            _favoriteRepository.Remove(username, mediaTitle);
        }

        public FavoriteList GetFavorites(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("username is required", nameof(username));

            username = username.Trim();

            var titles = _favoriteRepository.GetFavoriteMediaTitles(username)
                .ToList();

            return new FavoriteList(username, titles);
        }
    }
}
