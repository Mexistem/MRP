using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using MRP.Server.Services;

namespace MRP.Server.Http.Handlers
{
    public sealed class RatingHandler
    {
        private readonly IRatingManager _ratingManager;
        private readonly IAuthManager _authManager;

        public RatingHandler(IRatingManager ratingManager, IAuthManager authManager)
        {
            _ratingManager = ratingManager ?? throw new ArgumentNullException(nameof(ratingManager));
            _authManager = authManager ?? throw new ArgumentNullException(nameof(authManager));
        }

        private static string? ExtractBearerToken(string? headerValue)
        {
            if (string.IsNullOrWhiteSpace(headerValue))
            {
                return null;
            }

            const string bearer = "Bearer ";
            if (headerValue.StartsWith(bearer, StringComparison.OrdinalIgnoreCase))
            {
                return headerValue.Substring(bearer.Length).Trim();
            }

            return headerValue.Trim();
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

        public async Task Create(RequestContext context)
        {
            context.Response.ContentType = "application/json";

            var usernameFromToken = await RequireUsernameFromTokenAsync(context);
            if (usernameFromToken is null)
            {
                return;
            }

            if (!string.Equals(context.Request.ContentType, "application/json", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid body" });
                return;
            }

            RatingCreateRequest? request;
            try
            {
                request = await JsonSerializer.DeserializeAsync<RatingCreateRequest>(context.Request.InputStream);
            }
            catch
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid body" });
                return;
            }

            if (request is null ||
                string.IsNullOrWhiteSpace(request.MediaTitle) ||
                string.IsNullOrWhiteSpace(request.Username))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid body" });
                return;
            }

            if (!string.Equals(request.Username.Trim(), usernameFromToken, StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "token user does not match username" });
                return;
            }

            try
            {
                var created = _ratingManager.CreateRating(
                    request.MediaTitle,
                    usernameFromToken,
                    request.Value,
                    request.Comment);

                context.Response.StatusCode = (int)HttpStatusCode.Created;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, created);
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
            catch (InvalidOperationException ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = ex.Message });
            }
            catch (Exception)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await JsonSerializer.SerializeAsync(
                    context.Response.OutputStream,
                    new { error = "media not found" }
                );
            }
        }

        public async Task GetAllForMedia(RequestContext context)
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
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid title" });
                return;
            }

            try
            {
                var ratings = _ratingManager.GetRatingsForMedia(title.Trim()).ToList();
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, ratings);
            }
            catch (KeyNotFoundException ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = ex.Message });
            }
        }

        public async Task GetForUser(RequestContext context)
        {
            context.Response.ContentType = "application/json";

            var usernameFromToken = await RequireUsernameFromTokenAsync(context);
            if (usernameFromToken is null)
            {
                return;
            }

            if (!context.Parameters.TryGetValue("title", out var title) || string.IsNullOrWhiteSpace(title) ||
                !context.Parameters.TryGetValue("username", out var username) || string.IsNullOrWhiteSpace(username))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid parameters" });
                return;
            }

            if (!string.Equals(username.Trim(), usernameFromToken, StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "not allowed" });
                return;
            }

            try
            {
                var rating = _ratingManager.GetRatingForUser(title.Trim(), usernameFromToken);
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, rating);
            }
            catch (KeyNotFoundException ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = ex.Message });
            }
        }

        public async Task Update(RequestContext context)
        {
            context.Response.ContentType = "application/json";

            var usernameFromToken = await RequireUsernameFromTokenAsync(context);
            if (usernameFromToken is null)
            {
                return;
            }

            if (!context.Parameters.TryGetValue("title", out var title) || string.IsNullOrWhiteSpace(title) ||
                !context.Parameters.TryGetValue("username", out var username) || string.IsNullOrWhiteSpace(username))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid parameters" });
                return;
            }

            if (!string.Equals(username.Trim(), usernameFromToken, StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "not allowed" });
                return;
            }

            if (!string.Equals(context.Request.ContentType, "application/json", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid body" });
                return;
            }

            RatingUpdateRequest? request;
            try
            {
                request = await JsonSerializer.DeserializeAsync<RatingUpdateRequest>(context.Request.InputStream);
            }
            catch
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid body" });
                return;
            }

            if (request is null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid body" });
                return;
            }

            try
            {
                var updated = _ratingManager.UpdateRating(
                    title.Trim(),
                    usernameFromToken,
                    request.Value,
                    request.Comment);

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, updated);
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

        public async Task GetAverageForMedia(RequestContext context)
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
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid title" });
                return;
            }

            try
            {
                var avg = _ratingManager.GetAverageRatingForMedia(title.Trim());
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { average = avg });
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

        public async Task Delete(RequestContext context)
        {
            context.Response.ContentType = "application/json";

            var usernameFromToken = await RequireUsernameFromTokenAsync(context);
            if (usernameFromToken is null)
            {
                return;
            }

            if (!context.Parameters.TryGetValue("title", out var title) || string.IsNullOrWhiteSpace(title) ||
                !context.Parameters.TryGetValue("username", out var username) || string.IsNullOrWhiteSpace(username))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid parameters" });
                return;
            }

            if (!string.Equals(username.Trim(), usernameFromToken, StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "not allowed" });
                return;
            }

            try
            {
                _ratingManager.DeleteRating(title.Trim(), usernameFromToken);
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { message = "rating deleted" });
            }
            catch (ArgumentException ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = ex.Message });
            }
        }
    }
}
