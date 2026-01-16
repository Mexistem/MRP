using System;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using MRP.Server.Services;

namespace MRP.Server.Http.Handlers
{
    public sealed class MediaHandler
    {
        private readonly IMediaManager _mediaManager;
        private readonly IAuthManager _authManager;

        public MediaHandler(IMediaManager mediaManager, IAuthManager authManager)
        {
            _mediaManager = mediaManager ?? throw new ArgumentNullException(nameof(mediaManager));
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

        private async Task<(string Username, string Role)?> RequireAuthFromTokenAsync(RequestContext context)
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

            var info = _authManager.GetTokenInfo(username);
            if (info is null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid or expired token" });
                return null;
            }

            if (!string.Equals(info.Token, token, StringComparison.Ordinal))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid or expired token" });
                return null;
            }

            if (info.ExpiresAt <= DateTime.UtcNow)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid or expired token" });
                return null;
            }

            var role = info.Role;
            if (string.IsNullOrWhiteSpace(role))
            {
                role = "User";
            }

            return (username, role);
        }

        public async Task GetAll(RequestContext context)
        {
            context.Response.ContentType = "application/json";

            var auth = await RequireAuthFromTokenAsync(context);
            if (auth is null)
            {
                return;
            }

            var entries = _mediaManager.GetAll().ToList();

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            await JsonSerializer.SerializeAsync(context.Response.OutputStream, entries);
        }

        public async Task Create(RequestContext context)
        {
            context.Response.ContentType = "application/json";

            var auth = await RequireAuthFromTokenAsync(context);
            if (auth is null)
            {
                return;
            }

            if (!string.Equals(context.Request.ContentType, "application/json", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid body" });
                return;
            }

            MediaCreateRequest? request;
            try
            {
                request = await JsonSerializer.DeserializeAsync<MediaCreateRequest>(context.Request.InputStream);
            }
            catch
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid body" });
                return;
            }

            if (request is null ||
                string.IsNullOrWhiteSpace(request.Title) ||
                string.IsNullOrWhiteSpace(request.Description) ||
                request.Genres is null ||
                string.IsNullOrWhiteSpace(request.CreatedBy))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid body" });
                return;
            }

            if (!string.Equals(request.CreatedBy.Trim(), auth.Value.Username, StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "token user does not match createdBy" });
                return;
            }

            try
            {
                var entry = _mediaManager.CreateMedia(
                    request.Title,
                    request.Description,
                    request.ReleaseYear,
                    request.Genres,
                    request.AgeRestriction,
                    request.Type,
                    auth.Value.Username);

                context.Response.StatusCode = (int)HttpStatusCode.Created;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, entry);
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
        }

        public async Task Update(RequestContext context)
        {
            context.Response.ContentType = "application/json";

            var auth = await RequireAuthFromTokenAsync(context);
            if (auth is null)
            {
                return;
            }

            if (!context.Parameters.TryGetValue("title", out var title) || string.IsNullOrWhiteSpace(title))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid title" });
                return;
            }

            var existing = _mediaManager.GetByTitle(title);
            if (existing is null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "media not found" });
                return;
            }

            var isAdmin = string.Equals(auth.Value.Role, "Admin", StringComparison.OrdinalIgnoreCase);
            var isOwner = string.Equals(existing.CreatedBy, auth.Value.Username, StringComparison.OrdinalIgnoreCase);
            if (!isAdmin && !isOwner)
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

            MediaUpdateRequest? request;
            try
            {
                request = await JsonSerializer.DeserializeAsync<MediaUpdateRequest>(context.Request.InputStream);
            }
            catch
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid body" });
                return;
            }

            if (request is null || request.Genres is null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid body" });
                return;
            }

            try
            {
                var updated = _mediaManager.UpdateMedia(
                    title: title,
                    newTitle: request.NewTitle,
                    description: request.Description,
                    releaseYear: request.ReleaseYear,
                    genres: request.Genres,
                    ageRestriction: request.AgeRestriction,
                    type: request.Type,
                    requestedBy: auth.Value.Username);

                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, updated);
            }
            catch (KeyNotFoundException ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "not allowed" });
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
        }

        public async Task Delete(RequestContext context)
        {
            context.Response.ContentType = "application/json";

            var auth = await RequireAuthFromTokenAsync(context);
            if (auth is null)
            {
                return;
            }

            if (!context.Parameters.TryGetValue("title", out var title) || string.IsNullOrWhiteSpace(title))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid title" });
                return;
            }

            var existing = _mediaManager.GetByTitle(title);
            if (existing is null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "media not found" });
                return;
            }

            var isAdmin = string.Equals(auth.Value.Role, "Admin", StringComparison.OrdinalIgnoreCase);
            var isOwner = string.Equals(existing.CreatedBy, auth.Value.Username, StringComparison.OrdinalIgnoreCase);
            if (!isAdmin && !isOwner)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "not allowed" });
                return;
            }

            try
            {
                _mediaManager.DeleteMedia(title, auth.Value.Username);
                context.Response.StatusCode = (int)HttpStatusCode.NoContent;
            }
            catch (KeyNotFoundException ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "not allowed" });
            }
        }
    }
}
