using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using MRP.Server.Services;

namespace MRP.Server.Http.Handlers
{
    public sealed class AuthHandler
    {
        private readonly IAuthManager _authManager;

        public AuthHandler(IAuthManager authManager)
        {
            _authManager = authManager;
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

        public async Task Login(RequestContext context)
        {
            context.Response.ContentType = "application/json";

            LoginRequest? request;
            try
            {
                request = await JsonSerializer.DeserializeAsync<LoginRequest>(context.Request.InputStream);
            }
            catch
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid body" });
                return;
            }

            if (request is null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid body" });
                return;
            }

            try
            {
                string token = _authManager.Login(request.Username.Trim(), request.Password.Trim());
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { token });
            }
            catch (UnauthorizedAccessException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid credentials" });
            }
            catch (InvalidOperationException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "unknown user" });
            }
        }

        public async Task Logout(RequestContext context)
        {
            context.Response.ContentType = "application/json";

            string? header = context.Request.Headers["Authorization"];
            if (string.IsNullOrWhiteSpace(header))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "missing token" });
                return;
            }

            string token = header.StartsWith("Bearer ") ? header.Substring(7).Trim() : header.Trim();
            string? username = _authManager.GetUsernameByToken(token);

            if (username is null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { error = "invalid or expired token" });
                return;
            }

            _authManager.Logout(username);

            context.Response.StatusCode = (int)HttpStatusCode.OK;
            await JsonSerializer.SerializeAsync(context.Response.OutputStream, new { message = "logout successful" });
        }

    }
}
