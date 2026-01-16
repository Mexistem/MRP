using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using MRP.Server.Http;
using MRP.Server.Services;

namespace MRP.Server.Http.Handlers
{
    public sealed class LikeHandler
    {
        private readonly ILikeManager _likeManager;
        private readonly IAuthManager _authManager;

        public LikeHandler(ILikeManager likeManager, IAuthManager authManager)
        {
            _likeManager = likeManager ?? throw new ArgumentNullException(nameof(likeManager));
            _authManager = authManager ?? throw new ArgumentNullException(nameof(authManager));
        }

        public async Task Like(RequestContext context)
        {
            context.Response.ContentType = "application/json";

            var likedBy = await RequireUsernameFromTokenAsync(context);
            if (likedBy is null)
            {
                return;
            }

            if (!context.Parameters.TryGetValue("title", out var title) ||
                !context.Parameters.TryGetValue("username", out var ratingUsername) ||
                string.IsNullOrWhiteSpace(title) ||
                string.IsNullOrWhiteSpace(ratingUsername))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid parameters" });
                return;
            }

            if (string.Equals(ratingUsername.Trim(), likedBy, StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "cannot like own rating" });
                return;
            }

            try
            {
                var result = _likeManager.LikeRating(title, ratingUsername, likedBy);
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, result);
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

        public async Task Unlike(RequestContext context)
        {
            context.Response.ContentType = "application/json";

            var likedBy = await RequireUsernameFromTokenAsync(context);
            if (likedBy is null)
            {
                return;
            }

            if (!context.Parameters.TryGetValue("title", out var title) ||
                !context.Parameters.TryGetValue("username", out var ratingUsername) ||
                string.IsNullOrWhiteSpace(title) ||
                string.IsNullOrWhiteSpace(ratingUsername))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid parameters" });
                return;
            }

            try
            {
                var result = _likeManager.UnlikeRating(title, ratingUsername, likedBy);
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, result);
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

        public async Task GetCount(RequestContext context)
        {
            context.Response.ContentType = "application/json";

            if (!context.Parameters.TryGetValue("title", out var title) ||
                !context.Parameters.TryGetValue("username", out var ratingUsername) ||
                string.IsNullOrWhiteSpace(title) ||
                string.IsNullOrWhiteSpace(ratingUsername))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid parameters" });
                return;
            }

            var count = _likeManager.GetLikeCount(title, ratingUsername);
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            await JsonSerializer.SerializeAsync(context.Response.OutputStream, new
            {
                mediaTitle = title.Trim().ToLowerInvariant(),
                ratingUsername = ratingUsername.Trim(),
                likeCount = count
            });
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
