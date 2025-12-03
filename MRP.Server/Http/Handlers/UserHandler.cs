using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using MRP.Server.Services;

namespace MRP.Server.Http.Handlers
{
    public sealed class UserHandler
    {
        private readonly IUserManager _userManager;
        private readonly IAuthManager _authManager;

        public UserHandler(IUserManager userManager, IAuthManager authManager)
        {
            _userManager = userManager;
            _authManager = authManager;
        }

        private static string? ExtractBearerToken(string? headerValue)
        {
            if (string.IsNullOrWhiteSpace(headerValue))
                return null;

            const string bearer = "Bearer ";
            if (headerValue.StartsWith(bearer, StringComparison.OrdinalIgnoreCase))
                return headerValue.Substring(bearer.Length).Trim();

            return headerValue.Trim();
        }

        public async Task Register(RequestContext context)
        {
            context.Response.ContentType = "application/json";

            if (!string.Equals(context.Request.ContentType, "application/json", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid body" });
                return;
            }

            RegisterRequest? request;
            try
            {
                request = await JsonSerializer.DeserializeAsync<RegisterRequest>(context.Request.InputStream);
            }
            catch
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid body" });
                return;
            }

            if (request is null ||
                string.IsNullOrWhiteSpace(request.Username) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid body" });
                return;
            }

            string username = request.Username.Trim();
            string password = request.Password.Trim();

            try
            {
                if (request.IsAdmin)
                    _userManager.RegisterAdmin(username, password);
                else
                    _userManager.Register(username, password);

                context.Response.StatusCode = (int)HttpStatusCode.Created;

                if (request.IsAdmin)
                    await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { message = "Admin created successfully" });
                else
                    await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { message = "User created successfully" });
            }
            catch (InvalidOperationException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                if (request.IsAdmin)
                    await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "Admin already exists" });
                else
                    await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "username already exists" });
            }
            catch (ArgumentException ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = ex.Message });
            }
        }

        public async Task Profile(RequestContext context)
        {
            context.Response.ContentType = "application/json";

            string? header = context.Request.Headers["Authorization"];
            string? token = ExtractBearerToken(header);

            if (string.IsNullOrWhiteSpace(token))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "missing bearer token" });
                return;
            }

            string? usernameFromToken = _authManager.GetUsernameByToken(token);
            if (usernameFromToken is null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid or expired token" });
                return;
            }

            var segments = context.Request.Url!.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            string? requestedUsername = segments.Length >= 3 ? segments[2] : null;

            if (requestedUsername is null ||
                !string.Equals(requestedUsername, usernameFromToken, StringComparison.Ordinal))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid or expired token" });
                return;
            }

            var user = _userManager.GetUser(usernameFromToken);
            if (user is null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "Not Found" });
                return;
            }

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            await JsonSerializer.SerializeAsync(context.Response.OutputStream, new
            {
                username = user.Username,
                role = user.Role,
                createdAt = user.CreatedAt
            });
        }
    }
}
