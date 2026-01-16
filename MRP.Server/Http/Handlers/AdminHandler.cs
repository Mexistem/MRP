using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using MRP.Server.Services;

namespace MRP.Server.Http.Handlers
{
    public sealed class AdminHandler
    {
        private readonly IUserManager _userManager;
        private readonly IAuthManager _authManager;

        public AdminHandler(IUserManager userManager, IAuthManager authManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
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

        public async Task DeleteUser(RequestContext context)
        {
            context.Response.ContentType = "application/json";

            var auth = await RequireAuthFromTokenAsync(context);
            if (auth is null)
            {
                return;
            }

            if (!string.Equals(auth.Value.Role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "admin required" });
                return;
            }

            if (!context.Parameters.TryGetValue("username", out var target) || string.IsNullOrWhiteSpace(target))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid username" });
                return;
            }

            try
            {
                _userManager.DeleteUserAsAdmin(target, auth.Value.Username);
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
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "admin required" });
            }
            catch (ArgumentException ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = ex.Message });
            }
        }
    }
}
