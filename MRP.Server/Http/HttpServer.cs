using MRP.Server.Http.Handlers;
using MRP.Server.Services;
using MRP.Server.Services.Interfaces;
using MRP.Server.Storage.Db;
using MRP.Server.Storage.InMemory;
using System;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MRP.Server.Http
{
    public sealed class HttpServer
    {
        private readonly HttpListener _listener = new();
        private readonly Router _router = new();

        public HttpServer(IUserManager userManager, IAuthManager authManager, 
                          IMediaManager mediaManager, IRatingManager ratingManager, 
                          ILikeManager likeManager, IFavoriteManager favoriteManager, ILeaderboardManager leaderboardManager)
        {
            var authHandler = new AuthHandler(authManager);
            var adminHandler = new AdminHandler(userManager, authManager);
            var statisticsHandler = new UserStatisticsManager(ratingManager, mediaManager, favoriteManager, likeManager);
            var userHandler = new UserHandler(userManager, authManager, statisticsHandler);
            var mediaHandler = new MediaHandler(mediaManager, authManager);
            var ratingHandler = new RatingHandler(ratingManager, authManager);
            var likeHandler = new LikeHandler(likeManager, authManager);
            var favoriteHandler = new FavoriteHandler(favoriteManager, authManager);
            var leaderboardHandler = new LeaderboardHandler(leaderboardManager, authManager);

            _router.Map("POST", "/api/users/register", userHandler.Register);
            _router.Map("POST", "/api/users/login", authHandler.Login);
            _router.Map("POST", "/api/users/logout", authHandler.Logout);
            _router.Map("GET", "/api/users/{username}/profile", userHandler.ProfilePublic);
            _router.Map("GET", "/api/users/{username}/profile/private", userHandler.ProfilePrivate);
            _router.Map("DELETE", "/api/admin/users/{username}", adminHandler.DeleteUser);

            _router.Map("GET", "/api/media", mediaHandler.GetAll);
            _router.Map("POST", "/api/media", mediaHandler.Create);
            _router.Map("PUT", "/api/media/{title}", mediaHandler.Update);
            _router.Map("DELETE", "/api/media/{title}", mediaHandler.Delete);

            _router.Map("POST", "/api/ratings", ratingHandler.Create);
            _router.Map("GET", "/api/ratings/{title}", ratingHandler.GetAllForMedia);
            _router.Map("GET", "/api/ratings/{title}/average", ratingHandler.GetAverageForMedia);
            _router.Map("GET", "/api/ratings/{title}/{username}", ratingHandler.GetForUser);
            _router.Map("PUT", "/api/ratings/{title}/{username}", ratingHandler.Update);
            _router.Map("DELETE", "/api/ratings/{title}/{username}", ratingHandler.Delete);

            _router.Map("POST", "/api/ratings/{title}/{username}/likes", likeHandler.Like);
            _router.Map("DELETE", "/api/ratings/{title}/{username}/likes", likeHandler.Unlike);
            _router.Map("GET", "/api/ratings/{title}/{username}/likes", likeHandler.GetCount);

            _router.Map("POST", "/api/media/{title}/favorite", favoriteHandler.Add);
            _router.Map("DELETE", "/api/media/{title}/favorite", favoriteHandler.Remove);
            _router.Map("GET", "/api/users/{username}/favorites", favoriteHandler.ListForUser);

            _router.Map("GET", "/api/leaderboard", leaderboardHandler.Get);

        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            _listener.Prefixes.Add($"http://localhost:8080/");
            _listener.Start();
            Console.WriteLine($"Http Server started on http://localhost:8080");

            while (!cancellationToken.IsCancellationRequested)
            {
                var httpContext = await _listener.GetContextAsync();
                var context = new RequestContext(httpContext);

                try
                {
                    if (!await _router.TryHandleAsync(context))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "Not Found" });
                    }
                }

                catch (Exception ex)
                {
                    try
                    {
                        context.Response.ContentType = "application/json";
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "internal server error" });
                    }
                    catch
                    {

                    }

                    Console.WriteLine(ex);
                }
                finally
                {
                    context.Response.OutputStream.Close();
                }
            }
        }
    }
}
