using MRP.Server.Services;
using MRP.Server.Services.Interfaces;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace MRP.Server.Http.Handlers
{
    public sealed class UserHandler
    {
        private readonly IUserManager _userManager;
        private readonly IAuthManager _authManager;
        private readonly IUserStatisticsHandler _statisticsHandler;

        public UserHandler(
            IUserManager userManager,
            IAuthManager authManager,
            IUserStatisticsHandler statisticsHandler)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _authManager = authManager ?? throw new ArgumentNullException(nameof(authManager));
            _statisticsHandler = statisticsHandler ?? throw new ArgumentNullException(nameof(statisticsHandler));
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

        private static string? GetRequestedUsernameFromUrl(RequestContext context)
        {
            var segments = context.Request.Url!.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length >= 3)
            {
                return segments[2];
            }

            return null;
        }

        private bool TryGetUsernameFromToken(RequestContext context, out string usernameFromToken, out bool missingToken)
        {
            usernameFromToken = "";
            missingToken = false;

            string? header = context.Request.Headers["Authorization"];
            string? token = ExtractBearerToken(header);

            if (string.IsNullOrWhiteSpace(token))
            {
                missingToken = true;
                return false;
            }

            var u = _authManager.GetUsernameByToken(token);
            if (u is null)
            {
                return false;
            }

            usernameFromToken = u;
            return true;
        }

        private static async Task WriteJson(RequestContext context, HttpStatusCode code, object payload)
        {
            context.Response.StatusCode = (int)code;
            context.Response.ContentType = "application/json";
            await JsonSerializer.SerializeAsync(context.Response.OutputStream, payload);
        }

        public async Task Register(RequestContext context)
        {
            context.Response.ContentType = "application/json";

            if (!string.Equals(context.Request.ContentType, "application/json", StringComparison.OrdinalIgnoreCase))
            {
                await WriteJson(context, HttpStatusCode.BadRequest, new { error = "invalid body" });
                return;
            }

            RegisterRequest? request;
            try
            {
                request = await JsonSerializer.DeserializeAsync<RegisterRequest>(context.Request.InputStream);
            }
            catch
            {
                await WriteJson(context, HttpStatusCode.BadRequest, new { error = "invalid body" });
                return;
            }

            if (request is null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                await WriteJson(context, HttpStatusCode.BadRequest, new { error = "invalid body" });
                return;
            }

            string username = request.Username.Trim();
            string password = request.Password.Trim();

            try
            {
                if (request.IsAdmin)
                {
                    _userManager.RegisterAdmin(username, password);
                }
                else
                {
                    _userManager.Register(username, password);
                }

                if (request.IsAdmin)
                {
                    await WriteJson(context, HttpStatusCode.Created, new { message = "Admin created successfully" });
                }
                else
                {
                    await WriteJson(context, HttpStatusCode.Created, new { message = "User created successfully" });
                }
            }
            catch (InvalidOperationException)
            {
                if (request.IsAdmin)
                {
                    await WriteJson(context, HttpStatusCode.Conflict, new { error = "Admin already exists" });
                }
                else
                {
                    await WriteJson(context, HttpStatusCode.Conflict, new { error = "username already exists" });
                }
            }
            catch (ArgumentException ex)
            {
                await WriteJson(context, HttpStatusCode.BadRequest, new { error = ex.Message });
            }
        }

        public async Task ProfilePublic(RequestContext context)
        {
            context.Response.ContentType = "application/json";

            if (!TryGetUsernameFromToken(context, out _, out var missingToken))
            {
                if (missingToken)
                {
                    await WriteJson(context, HttpStatusCode.Unauthorized, new { error = "missing bearer token" });
                    return;
                }

                await WriteJson(context, HttpStatusCode.Unauthorized, new { error = "invalid or expired token" });
                return;
            }

            var requestedUsername = GetRequestedUsernameFromUrl(context);
            if (string.IsNullOrWhiteSpace(requestedUsername))
            {
                await WriteJson(context, HttpStatusCode.BadRequest, new { error = "invalid request" });
                return;
            }

            var user = _userManager.GetUser(requestedUsername);
            if (user is null)
            {
                await WriteJson(context, HttpStatusCode.NotFound, new { error = "Not Found" });
                return;
            }

            var stats = _statisticsHandler.ComputePublic(requestedUsername);

            await WriteJson(context, HttpStatusCode.OK, new
            {
                username = user.Username,
                createdAt = user.CreatedAt,
                statistics = stats
            });
        }

        public async Task ProfilePrivate(RequestContext context)
        {
            context.Response.ContentType = "application/json";

            if (!TryGetUsernameFromToken(context, out var usernameFromToken, out var missingToken))
            {
                if (missingToken)
                {
                    await WriteJson(context, HttpStatusCode.Unauthorized, new { error = "missing bearer token" });
                    return;
                }

                await WriteJson(context, HttpStatusCode.Unauthorized, new { error = "invalid or expired token" });
                return;
            }

            var requestedUsername = GetRequestedUsernameFromUrl(context);
            if (string.IsNullOrWhiteSpace(requestedUsername) ||
                !string.Equals(requestedUsername, usernameFromToken, StringComparison.Ordinal))
            {
                await WriteJson(context, HttpStatusCode.Unauthorized, new { error = "invalid or expired token" });
                return;
            }

            var user = _userManager.GetUser(usernameFromToken);
            if (user is null)
            {
                await WriteJson(context, HttpStatusCode.NotFound, new { error = "Not Found" });
                return;
            }

            var stats = _statisticsHandler.ComputePrivate(usernameFromToken);

            await WriteJson(context, HttpStatusCode.OK, new
            {
                username = user.Username,
                role = user.Role,
                createdAt = user.CreatedAt,
                statistics = stats
            });
        }

        public Task Profile(RequestContext context)
        {
            return ProfilePrivate(context);
        }
    }
}
