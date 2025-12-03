using MRP.Server.Http.Handlers;
using MRP.Server.Services;
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

        public HttpServer(IUserManager userManager, IAuthManager authManager)
        {
            var authHandler = new AuthHandler(authManager);
            var userHandler = new UserHandler(userManager, authManager);

            _router.Map("POST", "/api/users/register", userHandler.Register);
            _router.Map("POST", "/api/users/login", authHandler.Login);
            _router.Map("POST", "/api/users/logout", authHandler.Logout);
            _router.Map("GET", "/api/users/{username}/profile", userHandler.Profile);

            var mediaRepository = new InMemoryMediaRepository();
            var mediaManager = new MediaManager(mediaRepository);
            var mediaHandler = new MediaHandler(mediaRepository, mediaManager);

            _router.Map("GET", "/api/media", mediaHandler.GetAll);
            _router.Map("POST", "/api/media", mediaHandler.Create);

            var ratingRepository = new InMemoryRatingRepository();
            var ratingManager = new RatingManager(ratingRepository);
            var ratingHandler = new RatingHandler(ratingManager, mediaRepository);

            _router.Map("POST", "/api/ratings", ratingHandler.Create);
            _router.Map("GET", "/api/ratings/{title}", ratingHandler.GetAllForMedia);

        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            _listener.Prefixes.Add("http://localhost:8080/");
            _listener.Start();
            Console.WriteLine("Http Server started on http://localhost:8080");

            while (!cancellationToken.IsCancellationRequested)
            {
                var httpContext = await _listener.GetContextAsync();
                var context = new RequestContext(httpContext);

                if (!await _router.TryHandleAsync(context))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "Not Found" });
                }

                context.Response.OutputStream.Close();
            }
        }
    }
}
