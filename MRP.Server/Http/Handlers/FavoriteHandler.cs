using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using MRP.Server.Services;

namespace MRP.Server.Http.Handlers
{
    public sealed class FavoriteHandler
    {
        private readonly IFavoriteManager _favoriteManager;
        private readonly IAuthManager _authManager;

        public FavoriteHandler(IFavoriteManager favoriteManager, IAuthManager authManager)
        {
            _favoriteManager = favoriteManager ?? throw new ArgumentNullException(nameof(favoriteManager));
            _authManager = authManager ?? throw new ArgumentNullException(nameof(authManager));
        }

        public async Task Add(RequestContext context)
        {
            context.Response.ContentType = "application/json";

            var usernameFromToken = await RequireUsernameFromTokenAsync(context);
            if (usernameFromToken is null)
            {
                return;
            }

            if (!context.Parameters.TryGetValue("title", out var title) || string.IsNullOrWhiteSpace(title))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid parameters" });
                return;
            }

            try
            {
                _favoriteManager.AddFavorite(usernameFromToken, title);
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { success = "favorite added" });
            }
            catch (KeyNotFoundException ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = ex.Message });
            }
        }

        public async Task Remove(RequestContext context)
        {
            context.Response.ContentType = "application/json";

            var username = await RequireUsernameFromTokenAsync(context);
            if (username is null)
            {
                return;
            }

            string? title = null;

            if (context.Parameters.TryGetValue("title", out var t1) && !string.IsNullOrWhiteSpace(t1))
            {
                title = t1;
            }

            if (title is null)
            {
                if (context.Parameters.TryGetValue("mediaTitle", out var t2) && !string.IsNullOrWhiteSpace(t2))
                {
                    title = t2;
                }
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid parameters" });
                return;
            }

            try
            {
                _favoriteManager.RemoveFavorite(username, title);

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { success = "favorite removed" });
            }
            catch (KeyNotFoundException ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = ex.Message });
            }
        }

        public async Task ListForUser(RequestContext context)
        {
            context.Response.ContentType = "application/json";

            var usernameFromToken = await RequireUsernameFromTokenAsync(context);
            if (usernameFromToken is null)
            {
                return;
            }

            if (!context.Parameters.TryGetValue("username", out var usernameInRoute) || string.IsNullOrWhiteSpace(usernameInRoute))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid parameters" });
                return;
            }

            if (!string.Equals(usernameInRoute.Trim(), usernameFromToken, StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid or expired token" });
                return;
            }

            var list = _favoriteManager.GetFavorites(usernameFromToken);
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            await JsonSerializer.SerializeAsync(context.Response.OutputStream, list);
        }

        private async Task<string?> RequireUsernameFromTokenAsync(RequestContext context)
        {
            string? header = context.Request.Headers["Authorization"];
            string? token = ExtractBearerToken(header);

            if (string.IsNullOrWhiteSpace(token))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "missing bearer token" });
                return null;
            }

            string? username = _authManager.GetUsernameByToken(token);
            if (username is null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid or expired token" });
                return null;
            }

            return username;
        }

        private static string? ExtractBearerToken(string? authorizationHeader)
        {
            if (string.IsNullOrWhiteSpace(authorizationHeader))
            {
                return null;
            }

            if (!authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return authorizationHeader["Bearer ".Length..].Trim();
        }
    }
}
